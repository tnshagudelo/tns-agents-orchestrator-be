using Domain.Interfaces;

namespace Application.Agents.AzureDevOps
{
    /// <summary>
    /// Facilita el logging de decisiones específicas del UnitTestAgent.
    /// </summary>
    public class UnitTestDecisionLogger
    {
        private readonly IDecisionLogger _logger;
        private const string AgentName = "UnitTestAgent";

        public UnitTestDecisionLogger(IDecisionLogger logger)
        {
            _logger = logger;
        }

        public Task LogToolCallAsync(
            Guid requestId,
            string toolName,
            Dictionary<string, string> parameters,
            string? result = null,
            string? reasoning = null,
            CancellationToken ct = default)
        {
            return _logger.LogAsync(new AgentDecision
            {
                RequestId = requestId,
                AgentName = AgentName,
                ToolCalled = toolName,
                Parameters = parameters,
                ToolResult = result != null && result.Length > 500
                    ? result[..500] + "... [truncado]"  // no guardes código completo en el log
                    : result,
                Reasoning = reasoning
            }, ct);
        }

        public Task LogStepAsync(Guid requestId, string step, string detail, CancellationToken ct = default)
        {
            return _logger.LogAsync(new AgentDecision
            {
                RequestId = requestId,
                AgentName = AgentName,
                ToolCalled = $"[STEP] {step}",
                ToolResult = detail
            }, ct);
        }
    }
}
