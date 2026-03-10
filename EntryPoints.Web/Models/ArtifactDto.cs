namespace EntryPoints.Web.Models
{
    public class ArtifactDto
    {
        public required string Type { get; init; }
        public required string Label { get; init; }
        public required string Value { get; init; }
    }
}
