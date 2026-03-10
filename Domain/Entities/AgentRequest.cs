namespace Domain.Entities
{
    /// <summary>
    /// Representa una petición normalizada que llega al sistema
    /// desde cualquier canal (Web o MCP/IDE).
    /// </summary>
    public class AgentRequest
    {
        /// <summary>
        /// ID único de esta petición. Se usa para trazabilidad y logs.
        /// </summary>
        public Guid RequestId { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Qué quiere hacer el usuario, en lenguaje natural.
        /// Ej: "genera tests para OrderService.cs"
        /// </summary>
        public required string UserMessage { get; init; }

        /// <summary>
        /// Qué tipo de agente debe manejar esta petición.
        /// El Dispatcher usa esto para enrutar.
        /// </summary>
        public required AgentType TargetAgent { get; init; }

        /// <summary>
        /// Desde dónde llegó la petición.
        /// </summary>
        public required RequestChannel Channel { get; init; }

        /// <summary>
        /// Quién hizo la petición (para logs y auditoría).
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// Datos adicionales específicos de cada agente.
        /// Ej: nombre del repo, path del archivo, etc.
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();

        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}
