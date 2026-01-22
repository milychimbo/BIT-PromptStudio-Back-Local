using BitPromptStudioBackend.Context;

using BitPromptStudioBackend.Services;
using BitPromptStudioBackend.Services.interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// agregar base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DatabaseConnection")
    )
);



builder.Services.Configure<AiFoundryOptions>(
    builder.Configuration.GetSection("AiFoundry")
);

builder.Services.AddScoped<IPromptService, PromptService>();

builder.Services.AddHttpClient<IAgenteService, AgenteService>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<AiFoundryOptions>>().Value;
    http.BaseAddress = new Uri(opt.Endpoint.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(20);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));
app.MapGet("/", () => Results.Ok("API BitPromptStudio is running!"));

app.Run();
