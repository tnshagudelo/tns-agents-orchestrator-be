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
    /// Plugin de escritura en Azure DevOps.
    /// ⚠ Este plugin tiene efectos reales en el repo.
    /// El Service Principal usado debe tener permisos mínimos:
    /// solo crear branches y PRs, nunca merge directo.
    /// </summary>
    public class DevOpsPlugin : IDevOpsWriter
    {
        private readonly AzureDevOpsConfig _config;
        private readonly ILogger<DevOpsPlugin> _logger;

        public DevOpsPlugin(AzureDevOpsConfig config, ILogger<DevOpsPlugin> logger)
        {
            _config = config;
            _logger = logger;
        }

        [KernelFunction("CreateBranch")]
        [Description("Crea un branch nuevo en el repositorio para subir los tests generados. Llama esto antes de hacer commit.")]
        public async Task<string> CreateBranchAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Nombre del branch. Formato: tests/{ClassName}-{fecha}")] string branchName,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[DevOps] Creando branch {Branch} en {Repo}", branchName, repoName);

            try
            {
                using var connection = BuildClient();
                var gitClient = connection.GetClient<GitHttpClient>();

                // Obtiene el commit más reciente de main para crear el branch desde ahí
                var defaultBranch = await gitClient.GetBranchAsync(
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    name: "master",
                    cancellationToken: ct
                );

                var newBranch = new GitRefUpdate
                {
                    Name = $"refs/heads/{branchName}",
                    OldObjectId = "0000000000000000000000000000000000000000",
                    NewObjectId = defaultBranch.Commit.CommitId
                };

                var result = await gitClient.UpdateRefsAsync(
                    refUpdates: new[] { newBranch },
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    cancellationToken: ct
                );

                if (result.Any(r => r.Success))
                {
                    _logger.LogInformation("[DevOps] Branch {Branch} creado exitosamente", branchName);
                    return $"Branch '{branchName}' creado exitosamente.";
                }

                return $"ERROR: No se pudo crear el branch '{branchName}'.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DevOps] Error creando branch {Branch}", branchName);
                return $"ERROR: {ex.Message}";
            }
        }

        [KernelFunction("CommitTestFile")]
        [Description("Sube el archivo de tests generado al branch. Llama esto después de CreateBranch y solo si la validación fue exitosa.")]
        public async Task CommitFileAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Nombre del branch donde subir el archivo")] string branchName,
            [Description("Path completo donde guardar el archivo. Ej: tests/Services/OrderServiceTests.cs")] string filePath,
            [Description("Contenido completo del archivo de tests generado")] string content,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[DevOps] Commiteando {FilePath} en {Branch}", filePath, branchName);

            try
            {
                using var connection = BuildClient();
                var gitClient = connection.GetClient<GitHttpClient>();

                // Obtiene el último commit del branch para hacer el push sobre él
                var branch = await gitClient.GetBranchAsync(
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    name: branchName,
                    cancellationToken: ct
                );

                var push = new GitPush
                {
                    RefUpdates = new[]
                    {
                    new GitRefUpdate
                    {
                        Name        = $"refs/heads/{branchName}",
                        OldObjectId = branch.Commit.CommitId
                    }
                },
                    Commits = new[]
                    {
                    new GitCommitRef
                    {
                        Comment = $"[UnitTestAgent] Agrega tests para {Path.GetFileNameWithoutExtension(filePath)}",
                        Changes = new[]
                        {
                            new GitChange
                            {
                                ChangeType = VersionControlChangeType.Add,
                                Item       = new GitItem { Path = filePath },
                                NewContent = new ItemContent
                                {
                                    Content     = content,
                                    ContentType = ItemContentType.RawText
                                }
                            }
                        }
                    }
                }
                };

                await gitClient.CreatePushAsync(
                    push: push,
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    cancellationToken: ct
                );

                _logger.LogInformation("[DevOps] Commit exitoso en {Branch}", branchName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DevOps] Error en commit a {Branch}", branchName);
                throw;
            }
        }

        [KernelFunction("CreatePullRequest")]
        [Description("Crea el Pull Request con los tests generados. Llama esto como último paso, después del commit exitoso.")]
        public async Task<string> CreatePullRequestAsync(
            [Description("Nombre del repositorio")] string repoName,
            [Description("Branch con los tests generados")] string branchName,
            [Description("Título del PR. Formato: [UnitTestAgent] Tests para {ClassName}")] string title,
            [Description("Descripción del PR explicando qué tests se generaron y por qué")] string description,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[DevOps] Creando PR desde {Branch}", branchName);

            try
            {
                using var connection = BuildClient();
                var gitClient = connection.GetClient<GitHttpClient>();

                var pr = new GitPullRequest
                {
                    Title=         title,
                    Description=   description,
                    SourceRefName= $"refs/heads/{branchName}",
                    TargetRefName= "refs/heads/master"
                };

                var created = await gitClient.CreatePullRequestAsync(
                    gitPullRequestToCreate: pr,
                    project: _config.ProjectName,
                    repositoryId: repoName,
                    cancellationToken: ct
                );

                var prUrl = $"{_config.OrganizationUrl}/{_config.ProjectName}/_git/{repoName}/pullrequest/{created.PullRequestId}";

                _logger.LogInformation("[DevOps] PR creado: {Url}", prUrl);

                return prUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DevOps] Error creando PR desde {Branch}", branchName);
                return $"ERROR: {ex.Message}";
            }
        }

        private VssConnection BuildClient()
        {
            var credentials = new VssBasicCredential(string.Empty, _config.PersonalAccessToken);
            return new VssConnection(new Uri(_config.OrganizationUrl), credentials);
        }
    }
}
