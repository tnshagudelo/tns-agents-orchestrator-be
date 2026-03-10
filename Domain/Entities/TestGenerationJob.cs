using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    /// <summary>
    /// Estado del trabajo de generación de tests.
    /// El UnitTestAgent actualiza esto mientras avanza.
    /// </summary>
    public class TestGenerationJob
    {
        public Guid JobId { get; init; } = Guid.NewGuid();
        public Guid RequestId { get; init; }

        public required string RepoName { get; init; }
        public required string TargetFilePath { get; init; }   // ej: src/Services/OrderService.cs
        public required string TargetClassName { get; init; }  // ej: OrderService

        public TestFramework? DetectedFramework { get; set; }
        public List<string> MethodsToTest { get; set; } = new();
        public string? GeneratedTestCode { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Pending;
        public string? BranchName { get; set; }
        public string? PullRequestUrl { get; set; }

        public List<string> Errors { get; set; } = new();
        public DateTime StartedAt { get; init; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }

}
