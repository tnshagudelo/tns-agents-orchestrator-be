namespace Infrastructure.Plugins
{
    public class AzureDevOpsConfig
    {
        public required string OrganizationUrl { get; init; }
        public required string ProjectName { get; init; }
        public required string PersonalAccessToken { get; init; }
    }
}
