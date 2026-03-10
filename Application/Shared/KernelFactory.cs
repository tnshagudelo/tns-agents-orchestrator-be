using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Application.Shared
{
    /// <summary>
    /// Construye y configura el Kernel de Semantic Kernel.
    /// Un Kernel por ejecución de agente, no uno compartido global.
    /// </summary>
    public static class KernelFactory
    {
        /// <summary>
        /// Crea un Kernel configurado con OpenAI.
        /// Los plugins se registran después, en cada AgentRunner.
        /// </summary>
        public static Kernel Create(KernelConfig config, ILoggerFactory loggerFactory)
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: config.DeploymentName,  // ej: "gpt-4o"
                apiKey: config.ApiKey
            );

            builder.Services.AddSingleton(loggerFactory);

            return builder.Build();
        }
    }
}
