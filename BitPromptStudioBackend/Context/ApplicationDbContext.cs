using BitPromptStudioBackend.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitPromptStudioBackend.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CategoriaEntity> Categoria => Set<CategoriaEntity>();
    }
}
