using Worker;
using Serilog;
using Infrastructure.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();
    })
    .UseSerilog((context, services, loggerConfig) =>
    {
        loggerConfig
            .ReadFrom.Configuration(context.Configuration)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    })
    .ConfigureServices((context, services) =>
    {
        // Lee la sección Infrastructure del appsettings
        var infraConfig = context.Configuration
            .GetSection("Infrastructure")
            .Get<InfrastructureConfig>()
            ?? throw new InvalidOperationException(
                "Falta la sección 'Infrastructure' en appsettings.json");

        // Registra todo Infrastructure (plugins, agentes, dispatcher)
        services.AddInfrastructure(infraConfig);

        // Registra el worker que escucha peticiones
        services.AddHostedService<AgentWorker>();
    })
    .Build();

// Corre el script SQL la primera vez si la tabla no existe
// En producción esto lo maneja un migration script separado
await EnsureDatabaseAsync(host.Services);

await host.RunAsync();

static async Task EnsureDatabaseAsync(IServiceProvider services)
{
    // Por ahora solo logueamos — en el paso de BD lo completamos
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Sistema iniciando. Verificando infraestructura...");
    await Task.CompletedTask;
}