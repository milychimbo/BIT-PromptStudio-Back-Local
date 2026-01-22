using BitPromptStudioBackend.Context;
using BitPromptStudioBackend.DTOs;
using BitPromptStudioBackend.Entities;
using BitPromptStudioBackend.Services.interfaces;
using Microsoft.EntityFrameworkCore;

namespace BitPromptStudioBackend.Services
{
    public class PromptService : IPromptService
    {
        private readonly ApplicationDbContext _context;

        public PromptService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PromptDto> CreatePromptAsync(CreatePromptDto dto)
        {
            // Business Rule: Only save if Quality Score >= 80
            if (dto.QualityScore < 80)
            {
                throw new InvalidOperationException($"No se puede guardar el prompt. La puntuación de calidad ({dto.QualityScore}%) es inferior al 80% requerido.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Create the Parent Prompt (Container) without BEST VERSION yet
                var prompt = new Prompt
                {
                    Id = Guid.NewGuid(),
                    Title = dto.Title,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedBy = dto.CreatedByUserId,
                    UpdatedAt = DateTime.UtcNow,
                    BestVersionId = null, // Break cycle: Start null
                    BestVersionScore = 0
                };
                
                _context.Prompts.Add(prompt);
                
                // Save first to establish Prompt existence
                await _context.SaveChangesAsync();

                // 2. Create the First Version
                var version = new PromptVersion
                {
                    Id = Guid.NewGuid(),
                    PromptId = prompt.Id,
                    VersionNumber = 1,
                    Content = dto.Content,
                    QualityScore = dto.QualityScore,
                    AuthorId = dto.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    AnatomyAnalysisJson = dto.AnatomyAnalysisJson,
                    DetectedIssuesJson = dto.DetectedIssuesJson,
                    SuggestionsJson = dto.SuggestionsJson
                };

                _context.PromptVersions.Add(version);
                await _context.SaveChangesAsync();

                // 3. Update Parent with link to Best Version
                prompt.BestVersionId = version.Id;
                prompt.BestVersionScore = version.QualityScore;

                // 4. Handle Tags
                if (dto.TagIds.Any())
                {
                    foreach (var tagId in dto.TagIds)
                    {
                        _context.PromptTags.Add(new PromptTag { PromptId = prompt.Id, TagId = tagId });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetPromptDetailAsync(prompt.Id) ?? throw new Exception("Error al recuperar el prompt creado");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<PromptDto>> GetFeedAsync(int page = 1, int pageSize = 20)
        {
            var query = _context.Prompts
                .Include(p => p.Creator)
                .Include(p => p.LastUpdater)
                .Include(p => p.BestVersion) // Include BestVersion for Content
                .Include(p => p.PromptTags).ThenInclude(pt => pt.Tag)
                .OrderByDescending(p => p.BestVersionScore)
                .ThenByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var prompts = await query.ToListAsync();

            return prompts.Select(MapToDto).ToList();
        }

        public async Task<PromptDto?> GetPromptDetailAsync(Guid id)
        {
            var prompt = await _context.Prompts
                .Include(p => p.Creator)
                .Include(p => p.LastUpdater)
                .Include(p => p.BestVersion) // Include BestVersion for Content
                .Include(p => p.PromptTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prompt == null) return null;

            return MapToDto(prompt);
        }

        public async Task<PromptDto> AddVersionAsync(Guid promptId, CreateVersionDto dto)
        {
            if (dto.QualityScore < 80)
                throw new InvalidOperationException($"No se puede guardar la versión. La puntuación de calidad ({dto.QualityScore}%) es inferior al 80%.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var prompt = await _context.Prompts
                    .Include(p => p.PromptTags)
                    .FirstOrDefaultAsync(p => p.Id == promptId)
                             ?? throw new KeyNotFoundException("No se encontró el prompt");

                // Update Metadata if provided
                if (!string.IsNullOrEmpty(dto.Title)) prompt.Title = dto.Title;
                if (dto.Description != null) prompt.Description = dto.Description;

                // Update Tags if provided
                if (dto.TagIds != null)
                {
                    // Remove existing
                    _context.PromptTags.RemoveRange(prompt.PromptTags);
                    
                    // Add new
                    foreach (var tagId in dto.TagIds)
                    {
                        _context.PromptTags.Add(new PromptTag { PromptId = prompt.Id, TagId = tagId });
                    }
                }

                // Calculate next version number
                var maxVersion = await _context.PromptVersions
                    .Where(v => v.PromptId == promptId)
                    .MaxAsync(v => (int?)v.VersionNumber) ?? 0;

                var newVersion = new PromptVersion
                {
                    Id = Guid.NewGuid(),
                    PromptId = promptId,
                    VersionNumber = maxVersion + 1,
                    Content = dto.Content,
                    QualityScore = dto.QualityScore,
                    AuthorId = dto.AuthorId,
                    CreatedAt = DateTime.UtcNow,
                    AnatomyAnalysisJson = dto.AnatomyAnalysisJson,
                    DetectedIssuesJson = dto.DetectedIssuesJson,
                    SuggestionsJson = dto.SuggestionsJson
                };

                _context.PromptVersions.Add(newVersion);
                
                // Save Version FIRST to ensure ID exists
                await _context.SaveChangesAsync();

                // Update Parent "Best Version" Logic
                // Interpreted: The FEED shows the BEST version. 
                if (dto.QualityScore > prompt.BestVersionScore)
                {
                    prompt.BestVersionId = newVersion.Id;
                    prompt.BestVersionScore = dto.QualityScore;
                    prompt.LastUpdatedBy = dto.AuthorId; // Winner takes the credit on the feed
                }
                
                prompt.UpdatedAt = DateTime.UtcNow; // Bump timestamp regardless

                // Save Parent Update SECOND
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetPromptDetailAsync(promptId) ?? throw new Exception("Error recuperando el prompt actualizado");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<PromptVersionDto>> GetPromptVersionsAsync(Guid promptId)
        {
            var versions = await _context.PromptVersions
                .Include(v => v.Author)
                .Where(v => v.PromptId == promptId)
                .OrderByDescending(v => v.VersionNumber)
                .ToListAsync();

            return versions.Select(v => new PromptVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Content = v.Content,
                QualityScore = v.QualityScore,
                CreatedAt = v.CreatedAt,
                AuthorName = v.Author?.FullName ?? "Desconocido",
                // Analysis details omitted for list view
                AnatomyAnalysisJson = null,
                DetectedIssuesJson = null,
                SuggestionsJson = null
            }).ToList();
        }

        public async Task<PromptVersionDto?> GetVersionDetailAsync(Guid versionId)
        {
            var v = await _context.PromptVersions
                .Include(v => v.Author)
                .FirstOrDefaultAsync(v => v.Id == versionId);

            if (v == null) return null;

            return new PromptVersionDto
            {
                Id = v.Id,
                VersionNumber = v.VersionNumber,
                Content = v.Content,
                QualityScore = v.QualityScore,
                CreatedAt = v.CreatedAt,
                AuthorName = v.Author?.FullName ?? "Desconocido",
                AnatomyAnalysisJson = v.AnatomyAnalysisJson,
                DetectedIssuesJson = v.DetectedIssuesJson,
                SuggestionsJson = v.SuggestionsJson
            };
        }

        private static PromptDto MapToDto(Prompt p)
        {
            return new PromptDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description ?? "",
                Content = p.BestVersion?.Content ?? "", // Map content from BestVersion
                ViewCount = p.ViewCount,
                UseCount = p.UseCount,
                BestVersionScore = p.BestVersionScore,
                UpdatedAt = p.UpdatedAt,
                CreatorName = p.Creator?.FullName ?? "Desconocido", // Spanish default
                LastUpdaterName = p.LastUpdater?.FullName ?? "Desconocido", // Spanish default
                Tags = p.PromptTags.Select(pt => new TagDto 
                { 
                    Id = pt.TagId, 
                    Name = pt.Tag?.Name ?? "", 
                    Color = pt.Tag?.Color ?? "" 
                }).ToList()
            };
        }
    }
}
