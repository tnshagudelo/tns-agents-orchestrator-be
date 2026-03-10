using Application.Agents.UnitTestAgent;
using Application.Orchestration;
using Application.Shared;
using Domain.Interfaces;
using Infrastructure.Logging;
using Infrastructure.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            InfrastructureConfig config)
        {
            // Config de Azure DevOps — compartida por los plugins
            var devOpsConfig = new AzureDevOpsConfig
            {
                OrganizationUrl = config.DevOpsOrganizationUrl,
                ProjectName = config.DevOpsProjectName,
                PersonalAccessToken = config.DevOpsPat
            };

            // Config del LLM
            var kernelConfig = new KernelConfig
            {
                ApiKey = config.OpenAiApiKey,
                DeploymentName = config.OpenAiModel   // ej: "gpt-4o"
            };

            // Plugins
            services.AddSingleton(devOpsConfig);
            services.AddSingleton<RepoReaderPlugin>();
            services.AddSingleton<DevOpsPlugin>();
            services.AddSingleton<CodeAnalyzerPlugin>();

            // Logger de decisiones
            services.AddSingleton<IDecisionLogger>(sp =>
                new SqlDecisionLogRepository(
                    config.SqlConnectionString,
                    sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SqlDecisionLogRepository>>()
                ));

            // Interfaces de dominio → implementaciones de infra
            services.AddSingleton<IRepoReader>(sp => sp.GetRequiredService<RepoReaderPlugin>());
            services.AddSingleton<IDevOpsWriter>(sp => sp.GetRequiredService<DevOpsPlugin>());

            // Kernel config
            services.AddSingleton(kernelConfig);

            // Agentes — registrados como IAgentRunner para que el Dispatcher los encuentre
            services.AddSingleton<IAgentRunner, UnitTestAgentRunner>();

            // Dispatcher
            services.AddSingleton<CoreDispatcher>();

            return services;
        }
    }
}
