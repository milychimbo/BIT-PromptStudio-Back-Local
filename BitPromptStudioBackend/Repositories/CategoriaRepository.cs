using BitPromptStudioBackend.Context;
using BitPromptStudioBackend.Entities;
using BitPromptStudioBackend.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitPromptStudioBackend.Repositories
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoriaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<CategoriaEntity>> GetAllAsync()
            => _context.Categoria.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync();

        public Task<CategoriaEntity?> GetByIdAsync(Guid id)
            => _context.Categoria.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<CategoriaEntity> AddAsync(CategoriaEntity entity)
        {
            _context.Categoria.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<CategoriaEntity> UpdateAsync(CategoriaEntity entity)
        {
            _context.Categoria.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Categoria.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return false;

            _context.Categoria.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> ExistsAsync(Guid id)
            => _context.Categoria.AnyAsync(x => x.Id == id);
    }
}
