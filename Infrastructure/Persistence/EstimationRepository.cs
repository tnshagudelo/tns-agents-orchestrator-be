using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Infrastructure.Persistence
{
    public class EstimationRepository : IEstimationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<EstimationRepository> _logger;

        public EstimationRepository(string connectionString, ILogger<EstimationRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task SaveAsync(ProjectEstimation estimation, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            INSERT INTO ProjectEstimations
                (Id, SessionId, ProjectName, CreatedBy, CreatedAt, MarkdownContent, Summary, Status)
            VALUES
                (@Id, @SessionId, @ProjectName, @CreatedBy, @CreatedAt, @MarkdownContent, @Summary, @Status)
            """;

            cmd.Parameters.AddWithValue("@Id", estimation.Id);
            cmd.Parameters.AddWithValue("@SessionId", estimation.SessionId);
            cmd.Parameters.AddWithValue("@ProjectName", estimation.ProjectName);
            cmd.Parameters.AddWithValue("@CreatedBy", estimation.CreatedBy);
            cmd.Parameters.AddWithValue("@CreatedAt", estimation.CreatedAt);
            cmd.Parameters.AddWithValue("@MarkdownContent", estimation.MarkdownContent);
            cmd.Parameters.AddWithValue("@Summary", estimation.Summary);
            cmd.Parameters.AddWithValue("@Status", estimation.Status.ToString());

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<ProjectEstimation?> GetBySessionAsync(Guid sessionId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            SELECT Id, SessionId, ProjectName, CreatedBy, CreatedAt, MarkdownContent, Summary, Status
            FROM ProjectEstimations
            WHERE SessionId = @SessionId
            """;

            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            return MapEstimation(reader);
        }

        public async Task<IEnumerable<ProjectEstimation>> GetAllAsync(
            string createdBy, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            SELECT Id, SessionId, ProjectName, CreatedBy, CreatedAt, MarkdownContent, Summary, Status
            FROM ProjectEstimations
            WHERE CreatedBy = @CreatedBy
            ORDER BY CreatedAt DESC
            """;

            cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

            var result = new List<ProjectEstimation>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
                result.Add(MapEstimation(reader));

            return result;
        }

        public async Task UpdateStatusAsync(
            Guid estimationId, EstimationStatus status, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            UPDATE ProjectEstimations SET Status = @Status WHERE Id = @Id
            """;

            cmd.Parameters.AddWithValue("@Status", status.ToString());
            cmd.Parameters.AddWithValue("@Id", estimationId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static ProjectEstimation MapEstimation(SqlDataReader reader) => new()
        {
            SessionId = reader.GetGuid(1),
            ProjectName = reader.GetString(2),
            CreatedBy = reader.GetString(3),
            CreatedAt = reader.GetDateTime(4),
            MarkdownContent = reader.GetString(5),
            Summary = reader.GetString(6),
            Status = Enum.Parse<EstimationStatus>(reader.GetString(7))
        };
    }
}


//CREATE TABLE ConversationMessages (
//    Id          UNIQUEIDENTIFIER PRIMARY KEY,
//    SessionId   UNIQUEIDENTIFIER NOT NULL,
//    Role        NVARCHAR(20)     NOT NULL,
//    Content     NVARCHAR(MAX)    NOT NULL,
//    Timestamp   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

//    INDEX IX_ConversationMessages_SessionId (SessionId)
//);

//CREATE TABLE ProjectEstimations (
//    Id              UNIQUEIDENTIFIER PRIMARY KEY,
//    SessionId       UNIQUEIDENTIFIER NOT NULL,
//    ProjectName     NVARCHAR(200)    NOT NULL,
//    CreatedBy       NVARCHAR(200)    NOT NULL,
//    CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
//    MarkdownContent NVARCHAR(MAX)    NOT NULL,
//    Summary         NVARCHAR(500)    NOT NULL,
//    Status          NVARCHAR(50)     NOT NULL DEFAULT 'Draft',

//    INDEX IX_ProjectEstimations_SessionId  (SessionId),
//    INDEX IX_ProjectEstimations_CreatedBy  (CreatedBy)
//);