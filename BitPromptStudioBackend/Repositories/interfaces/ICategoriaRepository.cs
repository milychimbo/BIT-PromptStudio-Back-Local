using BitPromptStudioBackend.Entities;

namespace BitPromptStudioBackend.Repositories.interfaces
{
    public interface ICategoriaRepository
    {
        Task<List<CategoriaEntity>> GetAllAsync();
        Task<CategoriaEntity?> GetByIdAsync(Guid id);
        Task<CategoriaEntity> AddAsync(CategoriaEntity entity);
        Task<CategoriaEntity> UpdateAsync(CategoriaEntity entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
