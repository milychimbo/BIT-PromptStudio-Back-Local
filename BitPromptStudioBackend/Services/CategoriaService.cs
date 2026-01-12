using BitPromptStudioBackend.Entities;
using BitPromptStudioBackend.Repositories.interfaces;
using BitPromptStudioBackend.Services.interfaces;

namespace BitPromptStudioBackend.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repo;

        public CategoriaService(ICategoriaRepository repo)
        {
            _repo = repo;
        }

        public Task<List<CategoriaEntity>> GetAllAsync() => _repo.GetAllAsync();

        public Task<CategoriaEntity?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);

        public async Task<CategoriaEntity> CreateAsync(string nombre, Guid padre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Nombre es requerido.");

            // Si padre != Empty, opcional: validar que exista
            if (padre != Guid.Empty && !await _repo.ExistsAsync(padre))
                throw new ArgumentException("La categoría padre no existe.");

            var entity = new CategoriaEntity
            {
                Id = Guid.NewGuid(),
                Nombre = nombre.Trim(),
                Padre = padre
            };

            return await _repo.AddAsync(entity);
        }

        public async Task<CategoriaEntity?> UpdateAsync(Guid id, string nombre, Guid padre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("Nombre es requerido.");

            if (!await _repo.ExistsAsync(id))
                return null;

            if (padre == id)
                throw new ArgumentException("Una categoría no puede ser su propio padre.");

            if (padre != Guid.Empty && !await _repo.ExistsAsync(padre))
                throw new ArgumentException("La categoría padre no existe.");

            var entity = new CategoriaEntity
            {
                Id = id,
                Nombre = nombre.Trim(),
                Padre = padre
            };

            return await _repo.UpdateAsync(entity);
        }

        public Task<bool> DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }
}
