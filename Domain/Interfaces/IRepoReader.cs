namespace Domain.Interfaces
{
    /// <summary>
    /// Contrato para leer contenido de un repositorio.
    /// Implementado en Infrastructure con Azure DevOps API.
    /// </summary>
    public interface IRepoReader
    {
        /// <summary>
        /// Obtiene el contenido completo de un archivo del repo.
        /// </summary>
        Task<string> GetFileContentAsync(
            string repoName,
            string filePath,
            CancellationToken ct = default);

        /// <summary>
        /// Lista los archivos de una carpeta.
        /// Devuelve mensaje descriptivo si la carpeta no existe.
        /// </summary>
        Task<IEnumerable<string>> ListFilesAsync(
            string repoName,
            string folderPath,
            CancellationToken ct = default);

        /// <summary>
        /// Busca todos los proyectos de tests en el repo.
        /// Convención: carpetas que terminan en 'Test'.
        /// </summary>
        Task<string> FindTestProjectsAsync(
            string repoName,
            CancellationToken ct = default);

        /// <summary>
        /// Busca si ya existe un archivo de tests para una clase
        /// dentro de un proyecto de tests específico.
        /// </summary>
        Task<string> FindExistingTestFileAsync(
            string repoName,
            string testProjectPath,
            string className,
            CancellationToken ct = default);
    }
}