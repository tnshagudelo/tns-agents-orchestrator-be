using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace Infrastructure.Logging
{
    /// <summary>
    /// Persiste decisiones del agente en SQL Server.
    /// Si no tienes SQL aún, puedes cambiar la implementación
    /// por una que escriba a un archivo o a App Insights
    /// sin tocar nada fuera de Infrastructure.
    /// </summary>
    public class SqlDecisionLogRepository : IDecisionLogger
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlDecisionLogRepository> _logger;

        public SqlDecisionLogRepository(string connectionString, ILogger<SqlDecisionLogRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task LogAsync(AgentDecision decision, CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                var cmd = conn.CreateCommand();
                cmd.CommandText = """
                INSERT INTO AgentDecisionLog
                    (RequestId, AgentName, ToolCalled, ToolResult, Reasoning, Timestamp)
                VALUES
                    (@RequestId, @AgentName, @ToolCalled, @ToolResult, @Reasoning, @Timestamp)
                """;

                cmd.Parameters.AddWithValue("@RequestId", decision.RequestId);
                cmd.Parameters.AddWithValue("@AgentName", decision.AgentName);
                cmd.Parameters.AddWithValue("@ToolCalled", decision.ToolCalled);
                cmd.Parameters.AddWithValue("@ToolResult", (object?)decision.ToolResult ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Reasoning", (object?)decision.Reasoning ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Timestamp", decision.Timestamp);

                await cmd.ExecuteNonQueryAsync(ct);
            }
            catch (Exception ex)
            {
                // El log nunca debe frenar la ejecución del agente
                _logger.LogError(ex, "[DecisionLog] Error persistiendo decisión para RequestId={Id}", decision.RequestId);
            }
        }
    }
}

//CREATE TABLE AgentDecisionLog (
//    Id          BIGINT IDENTITY PRIMARY KEY,
//    RequestId   UNIQUEIDENTIFIER NOT NULL,
//    AgentName   NVARCHAR(100)    NOT NULL,
//    ToolCalled  NVARCHAR(200)    NOT NULL,
//    ToolResult  NVARCHAR(MAX)    NULL,
//    Reasoning   NVARCHAR(MAX)    NULL,
//    Timestamp   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),

//    INDEX IX_AgentDecisionLog_RequestId (RequestId)
//);