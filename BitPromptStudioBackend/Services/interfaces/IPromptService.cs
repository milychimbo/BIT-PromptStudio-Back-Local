using BitPromptStudioBackend.DTOs;
using BitPromptStudioBackend.Entities;

namespace BitPromptStudioBackend.Services.interfaces
{
    public interface IPromptService
    {
        Task<PromptDto> CreatePromptAsync(CreatePromptDto dto);
        Task<List<PromptDto>> GetFeedAsync(int page = 1, int pageSize = 20);
        Task<PromptDto?> GetPromptDetailAsync(Guid id);
        Task<PromptDto> AddVersionAsync(Guid promptId, CreateVersionDto dto);
    }
}
