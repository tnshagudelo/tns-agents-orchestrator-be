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
        Task<string> GetFileContentAsync(string repoName, string filePath, CancellationToken ct = default);

        /// <summary>
        /// Lista los archivos de una carpeta.
        /// </summary>
        Task<IEnumerable<string>> ListFilesAsync(string repoName, string folderPath, CancellationToken ct = default);

        /// <summary>
        /// Busca si ya existe un archivo de tests para una clase dada.
        /// Ej: busca OrderServiceTests.cs en la carpeta de tests.
        /// </summary>
        Task<string?> FindExistingTestFileAsync(string repoName, string className, CancellationToken ct = default);
    }
}