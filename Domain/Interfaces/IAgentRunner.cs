using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Contrato base que debe implementar todo agente del sistema.
    /// El Dispatcher trabaja con esta interfaz, no con implementaciones concretas.
    /// </summary>
    public interface IAgentRunner
    {
        /// <summary>
        /// El tipo de agente que implementa este runner.
        /// El Dispatcher usa esto para encontrar el runner correcto.
        /// </summary>
        AgentType AgentType { get; }

        /// <summary>
        /// Ejecuta el agente con la petición recibida.
        /// Siempre devuelve un AgentResult, nunca lanza excepción al caller.
        /// </summary>
        Task<AgentResult> RunAsync(AgentRequest request, CancellationToken ct = default);
    }
}
