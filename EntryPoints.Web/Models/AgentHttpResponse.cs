namespace EntryPoints.Web.Models
{
    /// <summary>
    /// Lo que la API devuelve al frontend.
    /// </summary>
    public class AgentHttpResponse
    {
        public Guid RequestId { get; init; }
        public bool Success { get; init; }
        public required string Summary { get; init; }
        public string? ErrorDetail { get; init; }
        public List<ArtifactDto> Artifacts { get; init; } = new();
        public DateTime CompletedAt { get; init; }
    }
}
