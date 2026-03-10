namespace Domain.Interfaces
{
    /// <summary>
    /// Registra cada decisión que toma el agente durante su ejecución.
    /// Implementado en Infrastructure con SQL o App Insights.
    /// </summary>
    public interface IDecisionLogger
    {
        /// <summary>
        /// Registra una decisión o acción que tomó el agente.
        /// </summary>
        Task LogAsync(AgentDecision decision, CancellationToken ct = default);
    }

    /// <summary>
    /// Una decisión concreta que tomó el agente.
    /// </summary>
    public class AgentDecision
    {
        public Guid RequestId { get; init; }
        public required string AgentName { get; init; }

        /// <summary>
        /// Qué herramienta/plugin decidió llamar. 
        /// Ej: "RepoReaderPlugin.GetFileContent"
        /// </summary>
        public required string ToolCalled { get; init; }

        /// <summary>
        /// Con qué parámetros la llamó.
        /// </summary>
        public Dictionary<string, string> Parameters { get; init; } = new();

        /// <summary>
        /// Qué devolvió la herramienta (resumido, no todo el código).
        /// </summary>
        public string? ToolResult { get; init; }

        /// <summary>
        /// Por qué el modelo decidió llamar esto (reasoning del LLM).
        /// </summary>
        public string? Reasoning { get; init; }

        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
