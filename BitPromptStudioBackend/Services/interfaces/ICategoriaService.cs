using BitPromptStudioBackend.Entities;

namespace BitPromptStudioBackend.Services.interfaces
{
    public interface ICategoriaService
    {
        Task<List<CategoriaEntity>> GetAllAsync();
        Task<CategoriaEntity?> GetByIdAsync(Guid id);
        Task<CategoriaEntity> CreateAsync(string nombre, Guid padre);
        Task<CategoriaEntity?> UpdateAsync(Guid id, string nombre, Guid padre);
        Task<bool> DeleteAsync(Guid id);
    }
}
