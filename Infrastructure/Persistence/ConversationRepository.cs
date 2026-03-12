using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Infrastructure.Persistence
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ConversationRepository> _logger;

        public ConversationRepository(string connectionString, ILogger<ConversationRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task SaveMessageAsync(ConversationMessage message, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            INSERT INTO ConversationMessages (Id, SessionId, Role, Content, Timestamp)
            VALUES (@Id, @SessionId, @Role, @Content, @Timestamp)
            """;

            cmd.Parameters.AddWithValue("@Id", message.Id);
            cmd.Parameters.AddWithValue("@SessionId", message.SessionId);
            cmd.Parameters.AddWithValue("@Role", message.Role);
            cmd.Parameters.AddWithValue("@Content", message.Content);
            cmd.Parameters.AddWithValue("@Timestamp", message.Timestamp);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IEnumerable<ConversationMessage>> GetHistoryAsync(
            Guid sessionId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = """
            SELECT Id, SessionId, Role, Content, Timestamp
            FROM ConversationMessages
            WHERE SessionId = @SessionId
            ORDER BY Timestamp ASC
            """;

            cmd.Parameters.AddWithValue("@SessionId", sessionId);

            var messages = new List<ConversationMessage>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {
                messages.Add(new ConversationMessage
                {
                    SessionId = reader.GetGuid(1),
                    Role = reader.GetString(2),
                    Content = reader.GetString(3)
                });
            }

            return messages;
        }

        public async Task ClearSessionAsync(Guid sessionId, CancellationToken ct = default)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ConversationMessages WHERE SessionId = @SessionId";
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public Task<bool> SessionExistsAsync(Guid sessionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
