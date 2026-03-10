namespace Domain.Interfaces
{
    public interface IDevOpsWriter
    {
        Task<string> CreateBranchAsync(string repoName, string branchName, CancellationToken ct = default);
        Task CommitFileAsync(string repoName, string branchName, string filePath, string content, CancellationToken ct = default);
        Task<string> CreatePullRequestAsync(string repoName, string branchName, string title, string description, CancellationToken ct = default);
    }
}
