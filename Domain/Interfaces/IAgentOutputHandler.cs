using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Maneja el output específico de un agente después de cada respuesta.
    /// Cada agente que necesite persistencia o procesamiento especial
    /// tiene su propia implementación.
    ///
    /// Ejemplos:
    /// - ProjectManagerAgent → persiste estimaciones
    /// - UnitTestAgent       → no necesita handler (no implementa)
    /// - CodeReviewAgent     → persiste reportes de revisión
    /// </summary>
    public interface IAgentOutputHandler
    {
        /// <summary>
        /// Tipo de agente que maneja este handler.
        /// El ConversationService lo usa para encontrar el handler correcto.
        /// </summary>
        AgentType AgentType { get; }

        /// <summary>
        /// Procesa el output del agente después de cada respuesta.
        /// Se llama automáticamente — el runner no sabe que existe.
        /// </summary>
        Task HandleAsync(
            AgentRequest request,
            Guid sessionId,
            string agentResponse,
            CancellationToken ct = default);
    }
}
