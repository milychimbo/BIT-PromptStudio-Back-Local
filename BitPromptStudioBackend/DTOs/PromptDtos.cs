using System;

namespace BitPromptStudioBackend.DTOs
{
    public class CreatePromptDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        
        // Tags IDs if sent from frontend
        public List<Guid> TagIds { get; set; } = new();

        // Optional analysis data if coming already from frontend
        public int QualityScore { get; set; }
        public string? AnatomyAnalysisJson { get; set; }
        public string? DetectedIssuesJson { get; set; }
        public string? SuggestionsJson { get; set; }

        // Creator (Could differ from token, but usually token)
        public Guid CreatedByUserId { get; set; }
    }

    public class CreateVersionDto
    {
        // Optional: Update Prompt Metadata
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<Guid>? TagIds { get; set; }

        public string Content { get; set; } = string.Empty;
        public int QualityScore { get; set; }
        
        public string? AnatomyAnalysisJson { get; set; }
        public string? DetectedIssuesJson { get; set; }
        public string? SuggestionsJson { get; set; }

        public Guid AuthorId { get; set; }
    }

    public class PromptDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public int ViewCount { get; set; }
        public int UseCount { get; set; }
        
        public int BestVersionScore { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Simplified User info
        public string CreatorName { get; set; } = string.Empty;
        public string LastUpdaterName { get; set; } = string.Empty;

        // Tags
        public List<TagDto> Tags { get; set; } = new();
    }

    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}
