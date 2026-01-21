using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitPromptStudioBackend.Entities
{
    public class Prompt
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int ViewCount { get; set; } = 0;
        public int UseCount { get; set; } = 0;

        // Best Version Cache
        public Guid? BestVersionId { get; set; }
        public int BestVersionScore { get; set; } = 0;

        // Audit
        public Guid CreatedBy { get; set; }
        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid LastUpdatedBy { get; set; }
        [ForeignKey("LastUpdatedBy")]
        public User? LastUpdater { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for BestVersion
        [ForeignKey("BestVersionId")]
        public PromptVersion? BestVersion { get; set; }

        // Navigation properties
        public ICollection<PromptVersion> Versions { get; set; } = new List<PromptVersion>();
        public ICollection<PromptTag> PromptTags { get; set; } = new List<PromptTag>();
    }
}
