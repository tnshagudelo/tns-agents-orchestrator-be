using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.ComponentModel;

namespace Infrastructure.Plugins
{
    /// <summary>
    /// Plugin de lectura del repo via Azure DevOps API.
    /// Solo lectura — nunca escribe nada.
    /// Los métodos con [KernelFunction] son visibles para el LLM.
    /// </summary>
    public class RepoReaderPlugin : IRepoReader
    {
        private readonly AzureDevOpsConfig _config;
        private readonly ILogger<RepoReaderPlugin> _logger;

        public RepoReaderPlugin(AzureDevOpsConfig config, ILogger<RepoReaderPlugin> logger)
        {
            _config = config;
            _logger = logger;
        }

        [KernelFunction("GetFileContent")]
        [Description("Lee el contenido completo de un archivo del repositorio. Úsalo primero antes de generar tests.")]
        public async Task<string> GetFileContentAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Path del archivo dentro del repo. Ej: src/Services/OrderService.cs")] string filePath,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[RepoReader] Leyendo {FilePath} en repo {Repo}", filePath, repoName);

            try
            {
                using var client = BuildClient();
                var gitClient = client.GetClient<GitHttpClient>();

                var item = await gitClient.GetItemAsync(
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    path: filePath,
                    includeContent: true,
                    cancellationToken: ct
                );

                return item.Content ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RepoReader] Error leyendo {FilePath}", filePath);
                return $"ERROR: No se pudo leer el archivo '{filePath}'. Detalle: {ex.Message}";
            }
        }

        [KernelFunction("ListFilesInFolder")]
        [Description("Lista los archivos dentro de una carpeta del repositorio.")]
        public async Task<IEnumerable<string>> ListFilesAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Path de la carpeta. Ej: src/Services")] string folderPath,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[RepoReader] Listando archivos en {Folder}", folderPath);

            try
            {
                using var client = BuildClient();
                var gitClient = client.GetClient<GitHttpClient>();

                var items = await gitClient.GetItemsAsync(
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    scopePath: folderPath,
                    recursionLevel: VersionControlRecursionType.OneLevel,
                    cancellationToken: ct
                );

                return items
                    .Where(i => !i.IsFolder)
                    .Select(i => i.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RepoReader] Error listando {Folder}", folderPath);
                return new[] { $"ERROR: {ex.Message}" };
            }
        }

        [KernelFunction("FindExistingTests")]
        [Description("Busca si ya existe un archivo de tests para una clase dada en el repositorio.")]
        public async Task<string?> FindExistingTestFileAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Nombre de la clase. Ej: OrderService")] string className,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[RepoReader] Buscando tests existentes para {Class}", className);

            try
            {
                using var client = BuildClient();
                var gitClient = client.GetClient<GitHttpClient>();

                // Busca en la carpeta de tests convencional
                var testFileName = $"{className}Tests.cs";

                var items = await gitClient.GetItemsAsync(
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    scopePath: "/",
                    recursionLevel: VersionControlRecursionType.Full,
                    cancellationToken: ct
                );

                var existing = items
                    .FirstOrDefault(i => i.Path.EndsWith(testFileName, StringComparison.OrdinalIgnoreCase));

                return existing?.Path;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RepoReader] Error buscando tests de {Class}", className);
                return null;
            }
        }

        private VssConnection BuildClient()
        {
            var credentials = new VssBasicCredential(string.Empty, _config.PersonalAccessToken);
            return new VssConnection(new Uri(_config.OrganizationUrl), credentials);
        }
    }
}
