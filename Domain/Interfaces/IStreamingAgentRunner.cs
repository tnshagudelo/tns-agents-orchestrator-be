using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Extiende IAgentRunner para agentes que soportan streaming.
    /// Es opcional — un agente puede implementar solo IAgentRunner
    /// si no necesita SSE.
    ///
    /// El endpoint de la API detecta si el runner implementa esta
    /// interfaz y decide si usa streaming o respuesta normal.
    /// </summary>
    public interface IStreamingAgentRunner : IAgentRunner
    {
        /// <summary>
        /// Ejecuta el agente y devuelve tokens uno por uno.
        /// Cada string del IAsyncEnumerable es un fragmento del response.
        /// </summary>
        IAsyncEnumerable<string> RunStreamingAsync(
            AgentRequest request,
            CancellationToken ct = default);
    }
}
