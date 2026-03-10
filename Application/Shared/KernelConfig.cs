namespace Application.Shared
{
    /// <summary>
    /// Configuración necesaria para construir el Kernel.
    /// Se inyecta desde appsettings, nunca hardcodeada.
    /// </summary>
    public class KernelConfig
    {
        public required string ApiKey { get; init; }
        public required string DeploymentName { get; init; } // ej: "gpt-4o"
    }
}
