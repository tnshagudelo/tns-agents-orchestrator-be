using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Shared
{
    /// <summary>
    /// Clase base para todos los agentes.
    /// Maneja errores, logs de inicio/fin y garantiza
    /// que siempre se devuelve un AgentResult.
    /// </summary>
    public abstract class BaseAgentRunner : IAgentRunner
    {
        protected readonly IDecisionLogger DecisionLogger;
        protected readonly ILogger Logger;

        protected BaseAgentRunner(IDecisionLogger decisionLogger, ILogger logger)
        {
            DecisionLogger = decisionLogger;
            Logger = logger;
        }

        public abstract AgentType AgentType { get; }

        /// <summary>
        /// Punto de entrada. Maneja el ciclo completo con
        /// error handling. No sobreescribas este método.
        /// </summary>
        public async Task<AgentResult> RunAsync(AgentRequest request, CancellationToken ct = default)
        {
            Logger.LogInformation(
                "[{Agent}] Iniciando. RequestId={RequestId} Channel={Channel} User={User}",
                AgentType, request.RequestId, request.Channel, request.UserName
            );

            try
            {
                var result = await ExecuteAsync(request, ct);

                Logger.LogInformation(
                    "[{Agent}] Completado. RequestId={RequestId} Success={Success}",
                    AgentType, request.RequestId, result.Success
                );

                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("[{Agent}] Cancelado. RequestId={RequestId}", AgentType, request.RequestId);
                return AgentResult.Fail(request.RequestId, "La operación fue cancelada.", "OperationCanceledException");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "[{Agent}] Error no manejado. RequestId={RequestId}",
                    AgentType, request.RequestId
                );

                return AgentResult.Fail(
                    request.RequestId,
                    "Ocurrió un error procesando tu solicitud.",
                    ex.Message
                );
            }
        }

        /// <summary>
        /// Cada agente implementa aquí su lógica específica.
        /// Si lanzas excepción aquí, BaseAgentRunner la captura.
        /// </summary>
        protected abstract Task<AgentResult> ExecuteAsync(AgentRequest request, CancellationToken ct);
    }
}
