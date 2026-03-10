namespace Domain.Entities
{
    /// <summary>
    /// Resultado de la ejecución de cualquier agente.
    /// </summary>
    public class AgentResult
    {
        public Guid RequestId { get; init; }
        public bool Success { get; init; }

        /// <summary>
        /// Mensaje legible para el usuario final.
        /// </summary>
        public required string Summary { get; init; }

        /// <summary>
        /// Error técnico si hubo falla. Null si fue exitoso.
        /// </summary>
        public string? ErrorDetail { get; init; }

        /// <summary>
        /// Links, paths, PRs, o cualquier output concreto
        /// que produjo el agente.
        /// </summary>
        public List<AgentArtifact> Artifacts { get; init; } = new();

        public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

        // Factories para no construir el objeto mal
        public static AgentResult Ok(Guid requestId, string summary, List<AgentArtifact>? artifacts = null)
            => new()
            {
                RequestId = requestId,
                Success = true,
                Summary = summary,
                Artifacts = artifacts ?? new()
            };

        public static AgentResult Fail(Guid requestId, string summary, string errorDetail)
            => new()
            {
                RequestId = requestId,
                Success = false,
                Summary = summary,
                ErrorDetail = errorDetail
            };
    }
}
