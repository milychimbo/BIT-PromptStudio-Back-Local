using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using BitPromptStudioBackend.Models;
using Microsoft.Extensions.Options;

namespace BitPromptStudioBackend.Services;

public sealed class AiFoundryOptions
{
    public string Endpoint { get; set; } = "";
    public string ProjectId { get; set; } = "";
    public string AssistantId { get; set; } = "";

    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}

public sealed class AgenteService : IAgenteService
{
    private const string ApiVersion = "2025-05-01";
    private readonly HttpClient _http;
    private readonly AiFoundryOptions _opt;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AgenteService(HttpClient http, IOptions<AiFoundryOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<PromptFixResult> FixPromptAsync(string userMessage, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            throw new ArgumentException("El mensaje no puede ir vacío.", nameof(userMessage));

        var token = await GetAccessTokenAsync(ct);

        // 1) Create Thread
        var threadId = await CreateThreadAsync(token, ct);

        // 2) Create Message
        await CreateUserMessageAsync(token, threadId, userMessage, ct);

        // 3) Create Run
        var runId = await CreateRunAsync(token, threadId, ct);

        // 4) Poll run until completed
        await WaitRunCompletedAsync(token, threadId, runId, ct);

        // 5) Read messages and extract assistant JSON
        var assistantText = await GetLatestAssistantTextAsync(token, threadId, ct);

        // 6) Parse JSON into your schema
        var json = ExtractFirstJsonObject(assistantText);
        var result = JsonSerializer.Deserialize<PromptFixResult>(json, JsonOpts)
                     ?? throw new InvalidOperationException("No se pudo deserializar el JSON del assistant.");


        // calcular promedio de calidad a partir del analisis_anatomia
        var anatomia = result.analisis_anatomia;

        var porcentajes = new[]
        {
            anatomia.rol.porcentaje,
            anatomia.objetivo.porcentaje,
            anatomia.alcance.porcentaje,
            anatomia.tono.porcentaje,
            anatomia.estructura.porcentaje,
            anatomia.uso_herramientas.porcentaje,
            anatomia.nivel_detalle.porcentaje,
            anatomia.interaccion_cierre.porcentaje
        };

        // evitar división por cero y valores fuera de rango
        var valoresValidos = porcentajes
            .Where(p => p >= 0 && p <= 100)
            .ToList();

        result.porcentaje_calidad = valoresValidos.Any()
            ? (int)Math.Round(valoresValidos.Average())
            : 0;

        return result;
    }

    // -------------------- Auth --------------------

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(15)); // ⏱️ CRÍTICO

        var cred = new ClientSecretCredential(
            _opt.TenantId,
            _opt.ClientId,
            _opt.ClientSecret
        );

        AccessToken token = await cred.GetTokenAsync(
            new TokenRequestContext(new[] { "https://ai.azure.com/.default" }),
            cts.Token
        );

        return token.Token;
    }

    // -------------------- HTTP Helpers --------------------

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativeUrl, string bearer)
    {
        var req = new HttpRequestMessage(method, relativeUrl);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return req;
    }

    // -------------------- Threads API calls --------------------

    private async Task<string> CreateThreadAsync(string bearer, CancellationToken ct)
    {
        var url = $"api/projects/{_opt.ProjectId}/threads?api-version={ApiVersion}";

        using var req = CreateRequest(HttpMethod.Post, url, bearer);
        req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"CreateThread falló ({(int)res.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetString()
               ?? throw new InvalidOperationException("La respuesta no trajo id de thread.");
    }

    private async Task CreateUserMessageAsync(string bearer, string threadId, string userMessage, CancellationToken ct)
    {
        var url = $"api/projects/{_opt.ProjectId}/threads/{threadId}/messages?api-version={ApiVersion}";

        var payload = new
        {
            role = "user",
            content = new object[]
            {
                new { type = "text", text = userMessage }
            }
        };

        var json = JsonSerializer.Serialize(payload, JsonOpts);

        using var req = CreateRequest(HttpMethod.Post, url, bearer);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"CreateMessage falló ({(int)res.StatusCode}): {body}");
    }

    private async Task<string> CreateRunAsync(string bearer, string threadId, CancellationToken ct)
    {
        var url = $"api/projects/{_opt.ProjectId}/threads/{threadId}/runs?api-version={ApiVersion}";

        var payload = new { assistant_id = _opt.AssistantId };
        var json = JsonSerializer.Serialize(payload, JsonOpts);

        using var req = CreateRequest(HttpMethod.Post, url, bearer);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"CreateRun falló ({(int)res.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetString()
               ?? throw new InvalidOperationException("La respuesta no trajo id de run.");
    }

    private async Task WaitRunCompletedAsync(string bearer, string threadId, string runId, CancellationToken ct)
    {
        var url = $"api/projects/{_opt.ProjectId}/threads/{threadId}/runs/{runId}?api-version={ApiVersion}";

        for (var i = 0; i < 60; i++)
        {
            using var req = CreateRequest(HttpMethod.Get, url, bearer);

            using var res = await _http.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"GetRun falló ({(int)res.StatusCode}): {body}");

            using var doc = JsonDocument.Parse(body);
            var status = doc.RootElement.GetProperty("status").GetString();

            if (string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.Equals(status, "failed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "expired", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"El run terminó en estado: {status}. Respuesta: {body}");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }

        throw new TimeoutException("El run no terminó a tiempo.");
    }

    private async Task<string> GetLatestAssistantTextAsync(string bearer, string threadId, CancellationToken ct)
    {
        var url = $"api/projects/{_opt.ProjectId}/threads/{threadId}/messages?api-version={ApiVersion}";

        using var req = CreateRequest(HttpMethod.Get, url, bearer);
        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"ListMessages falló ({(int)res.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);

        var data = doc.RootElement.GetProperty("data");

        for (int i = 0; i < data.GetArrayLength(); i++)
        {
            var msg = data[i];
            var role = msg.GetProperty("role").GetString();
            if (!string.Equals(role, "assistant", StringComparison.OrdinalIgnoreCase))
                continue;

            var contentArr = msg.GetProperty("content");

            for (int j = 0; j < contentArr.GetArrayLength(); j++)
            {
                var c = contentArr[j];
                if (c.GetProperty("type").GetString() == "text")
                {
                    if (c.TryGetProperty("text", out var t))
                    {
                        if (t.ValueKind == JsonValueKind.Object && t.TryGetProperty("value", out var v))
                            return v.GetString() ?? "";

                        if (t.ValueKind == JsonValueKind.String)
                            return t.GetString() ?? "";
                    }
                }
            }
        }

        throw new InvalidOperationException("No encontré texto del assistant en los mensajes.");
    }

    // -------------------- Helpers --------------------

    private static string ExtractFirstJsonObject(string text)
    {
        text = text.Trim();

        if (LooksLikeJson(text)) return StripCodeFences(text);

        var s = StripCodeFences(text);
        int start = s.IndexOf('{');
        if (start < 0) throw new InvalidOperationException("No encontré un objeto JSON en la respuesta del assistant.");

        int depth = 0;
        for (int i = start; i < s.Length; i++)
        {
            if (s[i] == '{') depth++;
            else if (s[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return s.Substring(start, i - start + 1);
            }
        }

        throw new InvalidOperationException("JSON incompleto: no cerraron las llaves.");
    }

    private static bool LooksLikeJson(string s)
    {
        s = StripCodeFences(s).Trim();
        return s.StartsWith("{") && s.EndsWith("}");
    }

    private static string StripCodeFences(string s)
    {
        s = s.Trim();
        if (s.StartsWith("```"))
        {
            var firstNewLine = s.IndexOf('\n');
            if (firstNewLine >= 0) s = s[(firstNewLine + 1)..];
            var endFence = s.LastIndexOf("```", StringComparison.Ordinal);
            if (endFence >= 0) s = s[..endFence];
        }
        return s.Trim();
    }
}
