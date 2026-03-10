namespace Infrastructure.DependencyInjection
{
    public class InfrastructureConfig
    {
        public required string OpenAiApiKey { get; init; }
        public required string OpenAiModel { get; init; }  // "gpt-4o"
        public required string DevOpsOrganizationUrl { get; init; }
        public required string DevOpsProjectName { get; init; }
        public required string DevOpsPat { get; init; }
        public required string SqlConnectionString { get; init; }
    }
}
