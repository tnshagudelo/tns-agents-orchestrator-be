using Application.Shared;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Application.Agents.UnitTestAgent
{
    /// <summary>
    /// Runner del UnitTestAgent.
    /// Instancia SK, registra plugins y ejecuta el loop agéntico.
    /// El LLM decide qué herramientas usar y en qué orden.
    /// </summary>
    public class UnitTestAgentRunner : BaseAgentRunner
    {
        private readonly KernelConfig _kernelConfig;
        private readonly ILoggerFactory _loggerFactory;
        private readonly UnitTestContextBuilder _contextBuilder;
        private readonly UnitTestDecisionLogger _decisionLogger;

        // Los plugins se inyectan como interfaces — la implementación real
        // vive en Infrastructure y se registra en el contenedor DI
        private readonly IRepoReader _repoReader;
        private readonly IDevOpsWriter _devOpsWriter;

        public override AgentType AgentType => AgentType.UnitTestAgent;

        public UnitTestAgentRunner(
            KernelConfig kernelConfig,
            ILoggerFactory loggerFactory,
            IDecisionLogger decisionLogger,
            IRepoReader repoReader,
            IDevOpsWriter devOpsWriter)
            : base(decisionLogger, loggerFactory.CreateLogger<UnitTestAgentRunner>())
        {
            _kernelConfig = kernelConfig;
            _loggerFactory = loggerFactory;
            _repoReader = repoReader;
            _devOpsWriter = devOpsWriter;
            _contextBuilder = new UnitTestContextBuilder();
            _decisionLogger = new UnitTestDecisionLogger(decisionLogger);
        }

        protected override async Task<AgentResult> ExecuteAsync(AgentRequest request, CancellationToken ct)
        {
            // 1. Valida y construye el job
            var job = _contextBuilder.BuildJob(request);
            await _decisionLogger.LogStepAsync(request.RequestId, "JobCreated",
                $"Clase: {job.TargetClassName} | Repo: {job.RepoName}", ct);

            // 2. Construye el Kernel con los plugins registrados
            var kernel = BuildKernel(request);

            // 3. Configura el comportamiento: el LLM llama herramientas automáticamente
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.2,   // bajo: queremos código predecible, no creativo
                MaxTokens = 4000
            };

            // 4. Arma el prompt con el contexto real del request
            var systemPrompt = _contextBuilder.BuildSystemPrompt(request);

            var fullPrompt = $"""
            {systemPrompt}

            ---
            Tarea actual:
            Archivo a analizar: {job.TargetFilePath}
            Clase objetivo: {job.TargetClassName}
            Repo: {job.RepoName}

            Inicia el proceso ahora.
            """;

            await _decisionLogger.LogStepAsync(request.RequestId, "PromptBuilt",
                $"Prompt listo para {job.TargetClassName}", ct);

            // 5. Ejecuta el loop agéntico
            // A partir de aquí el LLM toma el control.
            // Llama herramientas, ve resultados, decide qué sigue.
            var response = await kernel.InvokePromptAsync(
                fullPrompt,
                new KernelArguments(executionSettings),
                cancellationToken: ct
            );

            var resultText = response.ToString();

            await _decisionLogger.LogStepAsync(request.RequestId, "AgentCompleted", resultText, ct);

            // 6. Evalúa si el agente completó exitosamente
            // El agente siempre incluye "PR creado:" en su respuesta si fue exitoso
            if (resultText.Contains("PR creado:", StringComparison.OrdinalIgnoreCase))
            {
                var prUrl = ExtractPrUrl(resultText);

                return AgentResult.Ok(
                    request.RequestId,
                    $"Tests generados y PR creado para {job.TargetClassName}.",
                    new List<AgentArtifact>
                    {
                    new() { Type = "PullRequest", Label = $"PR: {job.TargetClassName}", Value = prUrl }
                    }
                );
            }

            // Si no hay PR en la respuesta, algo falló en el proceso
            return AgentResult.Fail(
                request.RequestId,
                "El agente no pudo completar el proceso.",
                resultText
            );
        }

        /// <summary>
        /// Construye el Kernel y registra los plugins disponibles para este agente.
        /// Solo registra lo que este agente necesita, no todos los plugins del sistema.
        /// </summary>
        private Kernel BuildKernel(AgentRequest request)
        {
            var kernel = KernelFactory.Create(_kernelConfig, _loggerFactory);

            // Registra los plugins como objetos — SK los inspecciona con reflection
            // y expone al LLM los métodos marcados con [KernelFunction]
            kernel.ImportPluginFromObject(_repoReader, "RepoReaderPlugin");
            kernel.ImportPluginFromObject(_devOpsWriter, "DevOpsPlugin");

            return kernel;
        }

        private static string ExtractPrUrl(string agentResponse)
        {
            // Busca el patrón "PR creado: https://..."
            var lines = agentResponse.Split('\n');
            var prLine = lines.FirstOrDefault(l =>
                l.Contains("PR creado:", StringComparison.OrdinalIgnoreCase));

            if (prLine == null) return "url-no-encontrada";

            var parts = prLine.Split(':', 2);
            return parts.Length > 1 ? parts[1].Trim() : "url-no-encontrada";
        }
    }
}
