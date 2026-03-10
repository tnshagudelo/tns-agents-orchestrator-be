using Application.Orchestration;
using Domain.Entities;

namespace Worker
{
    public class AgentWorker(
        CoreDispatcher dispatcher,
        ILogger<AgentWorker> logger,
        IHostEnvironment env) : BackgroundService
    {
        private readonly CoreDispatcher _dispatcher = dispatcher;
        private readonly ILogger<AgentWorker> _logger = logger;
        private readonly IHostEnvironment _env = env;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("AgentWorker iniciado en modo {Mode}",
                _env.IsDevelopment() ? "DESARROLLO (consola)" : "PRODUCCIÓN (cola)");

            if (_env.IsDevelopment())
                await RunConsoleModeAsync(ct);
            else
                await RunQueueModeAsync(ct);
        }

        /// <summary>
        /// Modo desarrollo: acepta peticiones escritas en consola.
        /// Útil para probar el agente sin infraestructura de colas.
        /// </summary>
        private async Task RunConsoleModeAsync(CancellationToken ct)
        {
            _logger.LogInformation("Modo consola activo. Escribe una petición y presiona Enter.");
            _logger.LogInformation("Ejemplo: genera tests para OrderService.cs en repo mi-repo");

            while (!ct.IsCancellationRequested)
            {
                Console.Write("\n> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                // Construye un request de prueba desde el input de consola
                var request = BuildTestRequest(input);

                _logger.LogInformation("Procesando RequestId={Id}", request.RequestId);

                var result = await _dispatcher.DispatchAsync(request, ct);

                if (result.Success)
                {
                    _logger.LogInformation("✓ Completado: {Summary}", result.Summary);
                    foreach (var artifact in result.Artifacts)
                        _logger.LogInformation("  → {Label}: {Value}", artifact.Label, artifact.Value);
                }
                else
                {
                    _logger.LogError("✗ Falló: {Summary}", result.Summary);
                    _logger.LogError("  Detalle: {Error}", result.ErrorDetail);
                }
            }
        }

        /// <summary>
        /// Modo producción: escucha una cola de mensajes.
        /// Aquí conectas Azure Service Bus, RabbitMQ o lo que uses.
        /// </summary>
        private async Task RunQueueModeAsync(CancellationToken ct)
        {
            _logger.LogInformation("Modo cola activo. Esperando mensajes...");

            // TODO PASO 5: conectar Service Bus aquí
            // Por ahora solo mantiene el worker vivo
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                _logger.LogDebug("Worker activo, esperando mensajes...");
            }
        }

        /// <summary>
        /// Construye un AgentRequest de prueba desde texto de consola.
        /// Solo para desarrollo — en producción el request viene de la cola.
        /// </summary>
        private static AgentRequest BuildTestRequest(string consoleInput)
        {
            // Parseo simple para pruebas
            // Formato esperado: "genera tests para {Clase}.cs en repo {repo}"
            // Ej: "genera tests para OrderService.cs en repo mi-repo"

            var parts = consoleInput.Split(' ');

            var className = parts.Length > 3 ? parts[3].Replace(".cs", "") : "MiClase";
            var repoName = parts.Length > 6 ? parts[6] : "mi-repo";

            return new AgentRequest
            {
                UserMessage = consoleInput,
                TargetAgent = AgentType.UnitTestAgent,
                Channel = RequestChannel.WebPortal,
                UserName = "dev-local",
                Metadata = new Dictionary<string, string>
                {
                    ["repoName"] = repoName,
                    ["projectName"] = "MiProyecto",
                    ["targetFilePath"] = $"src/Services/{className}.cs",
                    ["targetClassName"] = className
                }
            };
        }
    }
}
