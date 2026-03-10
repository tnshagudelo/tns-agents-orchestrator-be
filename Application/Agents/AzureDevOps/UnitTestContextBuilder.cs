using Domain.Entities;
using System.Reflection;

namespace Application.Agents.AzureDevOps
{
    /// <summary>
    /// Construye el contexto que necesita el UnitTestAgent:
    /// system prompt con datos reales del request.
    /// </summary>
    public class UnitTestContextBuilder
    {
        /// <summary>
        /// Lee el system prompt y reemplaza los placeholders
        /// con los datos del request actual.
        /// </summary>
        public string BuildSystemPrompt(AgentRequest request)
        {
            var template = LoadPromptTemplate();

            var repoName = request.Metadata.GetValueOrDefault("repoName", "no-especificado");
            var projectName = request.Metadata.GetValueOrDefault("projectName", "no-especificado");

            return template
                .Replace("{{repoName}}", repoName)
                .Replace("{{projectName}}", projectName)
                .Replace("{{userName}}", request.UserName);
        }

        /// <summary>
        /// Extrae los datos del trabajo desde el Metadata del request.
        /// Falla rápido si faltan datos obligatorios.
        /// </summary>
        public TestGenerationJob BuildJob(AgentRequest request)
        {
            if (!request.Metadata.TryGetValue("repoName", out var repoName))
                throw new ArgumentException("Metadata debe incluir 'repoName'");

            if (!request.Metadata.TryGetValue("targetFilePath", out var targetFilePath))
                throw new ArgumentException("Metadata debe incluir 'targetFilePath'");

            if (!request.Metadata.TryGetValue("targetClassName", out var targetClassName))
                throw new ArgumentException("Metadata debe incluir 'targetClassName'");

            return new TestGenerationJob
            {
                RequestId = request.RequestId,
                RepoName = repoName,
                TargetFilePath = targetFilePath,
                TargetClassName = targetClassName
            };
        }

        private static string LoadPromptTemplate()
        {
            // Lee el archivo embebido en el assembly
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Application.Agents.UnitTestAgent.Prompts.SystemPrompt.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"No se encontró el recurso: {resourceName}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}