using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitPromptStudioBackend.Entities
{
    public class UserFavorite
    {
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public Guid PromptId { get; set; }
        [ForeignKey("PromptId")]
        public Prompt? Prompt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
