using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Application.Shared
{
    /// <summary>
    /// Frena el agente si supera el límite de llamadas a herramientas.
    /// Evita loops infinitos en producción.
    /// </summary>
    public class ToolCallLimitFilter : IFunctionInvocationFilter
    {
        private readonly int _maxCalls;
        private int _callCount;
        private readonly ILogger _logger;

        public ToolCallLimitFilter(int maxCalls, ILogger logger)
        {
            _maxCalls = maxCalls;
            _callCount = 0;
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            _callCount++;

            _logger.LogInformation(
                "[ToolCallLimit] Llamada {Current}/{Max} → {Plugin}-{Function}",
                _callCount, _maxCalls,
                context.Function.PluginName,
                context.Function.Name
            );

            if (_callCount > _maxCalls)
            {
                _logger.LogError(
                    "[ToolCallLimit] Límite de {Max} llamadas superado. Abortando para evitar loop.",
                    _maxCalls
                );

                // Devuelve un resultado que el agente pueda leer y con el que pueda terminar
                context.Result = new FunctionResult(
                    context.Function,
                    "ERROR_LOOP: Se superó el límite de operaciones permitidas. " +
                    "Detén el proceso y reporta qué paso no pudo completarse."
                );

                return; // No llama next() — bloquea la ejecución real
            }

            await next(context);
        }
    }
}
