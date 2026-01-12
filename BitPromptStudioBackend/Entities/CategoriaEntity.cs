namespace BitPromptStudioBackend.Entities
{
    public class CategoriaEntity
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public Guid Padre { get; set; } = Guid.Empty;
    }
}
