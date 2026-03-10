using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Orchestration
{
    /// <summary>
    /// Recibe el request y lo delega al agente correcto.
    /// No tiene lógica de negocio. Solo enruta.
    /// </summary>
    public class CoreDispatcher
    {
        private readonly IReadOnlyDictionary<AgentType, IAgentRunner> _runners;
        private readonly ILogger<CoreDispatcher> _logger;

        /// <summary>
        /// Recibe todos los runners registrados en DI.
        /// Cada runner se registra con su AgentType como clave.
        /// </summary>
        public CoreDispatcher(IEnumerable<IAgentRunner> runners, ILogger<CoreDispatcher> logger)
        {
            _runners = runners.ToDictionary(r => r.AgentType);
            _logger = logger;
        }

        public async Task<AgentResult> DispatchAsync(AgentRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[Dispatcher] Recibido RequestId={RequestId} TargetAgent={Agent}",
                request.RequestId, request.TargetAgent
            );

            if (!_runners.TryGetValue(request.TargetAgent, out var runner))
            {
                _logger.LogError("[Dispatcher] No hay runner para {Agent}", request.TargetAgent);

                return AgentResult.Fail(
                    request.RequestId,
                    $"No existe un agente configurado para '{request.TargetAgent}'.",
                    "AgentNotFound"
                );
            }

            return await runner.RunAsync(request, ct);
        }
    }
}
