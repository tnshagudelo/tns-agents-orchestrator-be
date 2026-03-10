namespace Domain.Entities
{
    /// <summary>
    /// Cualquier output concreto del agente: un PR, un archivo, un link.
    /// </summary>
    public class AgentArtifact
    {
        public required string Type { get; init; }   // "PullRequest", "File", "Link"
        public required string Label { get; init; }  // texto para mostrar al usuario
        public required string Value { get; init; }  // la URL o el path real
    }
}
