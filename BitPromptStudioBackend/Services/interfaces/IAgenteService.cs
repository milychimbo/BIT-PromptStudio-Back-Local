using BitPromptStudioBackend.Models;

namespace BitPromptStudioBackend.Services
{
    public interface IAgenteService
    {
        /// <summary>
        /// Recibe un prompt original y devuelve el análisis y prompt mejorado
        /// usando Azure AI Foundry (threads + runs).
        /// </summary>
        Task<PromptFixResult> FixPromptAsync(
            string promptOriginal,
            CancellationToken cancellationToken = default
        );
    }
}
