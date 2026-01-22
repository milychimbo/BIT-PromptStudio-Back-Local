using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BitPromptStudioBackend.Entities
{
    public class PromptTag
    {
        public Guid PromptId { get; set; }
        [ForeignKey("PromptId")]
        public Prompt? Prompt { get; set; }

        public Guid TagId { get; set; }
        [ForeignKey("TagId")]
        public Tag? Tag { get; set; }
    }
}
