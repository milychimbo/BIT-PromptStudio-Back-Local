namespace BitPromptStudioBackend.Models
{
    public class CategoriaCreateRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public Guid Padre { get; set; } = Guid.Empty;
    }

    public class CategoriaUpdateRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public Guid Padre { get; set; } = Guid.Empty;
    }
}
