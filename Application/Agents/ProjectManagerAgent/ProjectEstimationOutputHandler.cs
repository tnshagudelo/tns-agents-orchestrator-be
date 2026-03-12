using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Agents.ProjectManagerAgent
{
    /// <summary>
    /// Maneja el output específico del ProjectManagerAgent.
    /// Se dispara automáticamente desde ConversationService
    /// cuando el agente genera una estimación completa.
    ///
    /// Para crear un handler para otro agente:
    /// 1. Crea una clase que implemente IAgentOutputHandler
    /// 2. Define AgentType con tu agente
    /// 3. Regístrala en DI — ConversationService la encuentra solo
    /// </summary>
    public class ProjectEstimationOutputHandler : IAgentOutputHandler
    {
        private readonly IEstimationRepository _estimationRepo;
        private readonly ILogger<ProjectEstimationOutputHandler> _logger;

        public AgentType AgentType => AgentType.ProjectManagerAgent;

        public ProjectEstimationOutputHandler(
            IEstimationRepository estimationRepo,
            ILogger<ProjectEstimationOutputHandler> logger)
        {
            _estimationRepo = estimationRepo;
            _logger = logger;
        }

        public async Task HandleAsync(
            AgentRequest request,
            Guid sessionId,
            string agentResponse,
            CancellationToken ct = default)
        {
            // Solo persiste si la respuesta contiene una estimación completa
            if (!agentResponse.Contains("ESTIMATION_START") ||
                !agentResponse.Contains("ESTIMATION_END"))
                return;

            var markdown = ExtractMarkdown(agentResponse);
            var projectName = request.Metadata.GetValueOrDefault("projectName", "Sin nombre");
            var existing = await _estimationRepo.GetBySessionAsync(sessionId, ct);

            if (existing != null)
            {
                await _estimationRepo.UpdateStatusAsync(
                    existing.Id, EstimationStatus.Complete, ct
                );
                _logger.LogInformation(
                    "[EstimationHandler] Estimación actualizada. SessionId={Id}", sessionId
                );
                return;
            }

            await _estimationRepo.SaveAsync(new ProjectEstimation
            {
                SessionId = sessionId,
                ProjectName = projectName,
                CreatedBy = request.UserName,
                MarkdownContent = markdown,
                Summary = ExtractSummary(markdown),
                Status = EstimationStatus.Complete
            }, ct);

            _logger.LogInformation(
                "[EstimationHandler] Estimación guardada. SessionId={Id}", sessionId
            );
        }

        private static string ExtractMarkdown(string content)
        {
            var start = content.IndexOf("ESTIMATION_START") + "ESTIMATION_START".Length;
            var end = content.IndexOf("ESTIMATION_END");
            return start >= 0 && end > start
                ? content[start..end].Trim()
                : content;
        }

        private static string ExtractSummary(string markdown)
            => string.Join(" ", markdown
                .Split('\n')
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(3))
                .Replace("#", "")
                .Trim();
    }
}