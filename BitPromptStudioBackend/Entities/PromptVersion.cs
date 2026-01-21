using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitPromptStudioBackend.Entities
{
    public class PromptVersion
    {
        [Key]
        public Guid Id { get; set; }

        public Guid PromptId { get; set; }
        
        [ForeignKey("PromptId")]
        public Prompt? Prompt { get; set; }

        public int VersionNumber { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public int QualityScore { get; set; } = 0;

        // JSON stored as strings
        public string? AnatomyAnalysisJson { get; set; }
        public string? DetectedIssuesJson { get; set; }
        public string? SuggestionsJson { get; set; }

        // Audit
        public Guid AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
