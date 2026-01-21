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

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Prompt> Prompts => Set<Prompt>();
        public DbSet<PromptVersion> PromptVersions => Set<PromptVersion>();
        public DbSet<PromptTag> PromptTags => Set<PromptTag>();
        public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PromptTag composite key
            modelBuilder.Entity<PromptTag>()
                .HasKey(pt => new { pt.PromptId, pt.TagId });

            modelBuilder.Entity<PromptTag>()
                .HasOne(pt => pt.Prompt)
                .WithMany(p => p.PromptTags)
                .HasForeignKey(pt => pt.PromptId);

            modelBuilder.Entity<PromptTag>()
                .HasOne(pt => pt.Tag)
                .WithMany()
                .HasForeignKey(pt => pt.TagId);

            // Configure UserFavorite composite key
            modelBuilder.Entity<UserFavorite>()
                .HasKey(uf => new { uf.UserId, uf.PromptId });
            
            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Avoid cycles

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Prompt)
                .WithMany()
                .HasForeignKey(uf => uf.PromptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship 1: Prompt has many Versions.
            modelBuilder.Entity<Prompt>()
                .HasMany(p => p.Versions)
                .WithOne(v => v.Prompt)
                .HasForeignKey(v => v.PromptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship 2: Prompt has ONE BestVersion (optional).
            modelBuilder.Entity<Prompt>()
                .HasOne(p => p.BestVersion)
                .WithMany()
                .HasForeignKey(p => p.BestVersionId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
