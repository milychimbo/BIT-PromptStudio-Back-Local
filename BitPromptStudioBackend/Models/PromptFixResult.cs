using System.Text.Json;

namespace BitPromptStudioBackend.Models
{
    public sealed class AnalisisItem
    {
        public int porcentaje { get; set; } = 0;
        public string comentario { get; set; } = string.Empty;
    }

    public sealed class AnalisisAnatomia
    {
        public AnalisisItem rol { get; set; } = new();
        public AnalisisItem objetivo { get; set; } = new();
        public AnalisisItem alcance { get; set; } = new();
        public AnalisisItem tono { get; set; } = new();
        public AnalisisItem estructura { get; set; } = new();
        public AnalisisItem uso_herramientas { get; set; } = new();
        public AnalisisItem nivel_detalle { get; set; } = new();
        public AnalisisItem interaccion_cierre { get; set; } = new();
    }

    public sealed class PromptFixResult
    {
        public string nombre_prompt { get; set; } = string.Empty;
        public string prompt_original { get; set; } = string.Empty;

        public AnalisisAnatomia analisis_anatomia { get; set; } = new();

        public JsonElement problemas_detectados { get; set; }
        // Array de strings

        public string prompt_mejorado { get; set; } = string.Empty;

        public JsonElement sugerencias { get; set; }
        // Array de strings

        public string nivel_calidad { get; set; } = string.Empty;
        // bajo | medio | alto

        public int porcentaje_calidad { get; set; } = 0;
        // promedio de analisis_anatomia
    }

    public sealed class FixPromptRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
