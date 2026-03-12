using Application.Orchestration;
using Domain.Interfaces;
using EntryPoints.Web.Mappers;
using EntryPoints.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace EntryPoints.Web.Controllers
{
    [ApiController]
    [Route("api/agent")]
    //[Authorize]
    public class AgentStreamController : ControllerBase
    {
        private readonly CoreDispatcher _dispatcher;
        private readonly ILogger<AgentStreamController> _logger;

        public AgentStreamController(
            CoreDispatcher dispatcher,
            ILogger<AgentStreamController> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint SSE — devuelve la respuesta del agente token por token.
        /// Angular se conecta con EventSource y pinta el texto mientras llega.
        ///
        /// Si el agente no soporta streaming, devuelve la respuesta completa
        /// como un solo evento — el FE no nota diferencia.
        /// </summary>
        [HttpPost("stream")]
        public async Task StreamAsync(
            [FromBody] AgentHttpRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return;
            }

            var userName = User.FindFirstValue(ClaimTypes.Name)
                        ?? User.FindFirstValue("preferred_username")
                        ?? "unknown";

            _logger.LogInformation(
                "[AgentStream] POST /stream Agent={Agent} User={User}",
                request.Agent, userName
            );

            // Configura los headers SSE
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            // Necesario para que Angular reciba los chunks en tiempo real
            Response.Headers["X-Accel-Buffering"] = "no";

            await Response.Body.FlushAsync(ct);

            try
            {
                var agentRequest = RequestNormalizer.ToAgentRequest(request, userName);
                var runner = _dispatcher.GetRunner(agentRequest.TargetAgent);

                if (runner is IStreamingAgentRunner streamingRunner)
                {
                    // Agente con soporte SSE — devuelve tokens uno por uno
                    await foreach (var token in streamingRunner.RunStreamingAsync(agentRequest, ct))
                    {
                        await WriteEventAsync(Response, "token", token, ct);
                    }
                }
                else
                {
                    // Agente sin SSE — ejecuta normal y devuelve como un evento único
                    var result = await runner.RunAsync(agentRequest, ct);
                    await WriteEventAsync(Response, "token", result.Summary, ct);
                }

                // Evento final — le dice al FE que el stream terminó
                await WriteEventAsync(Response, "done", "stream_complete", ct);
            }
            catch (ArgumentException ex)
            {
                await WriteEventAsync(Response, "error", ex.Message, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentStream] Error en stream");
                await WriteEventAsync(Response, "error", "Error procesando la solicitud.", ct);
            }
        }

        private static async Task WriteEventAsync(
            HttpResponse response,
            string eventType,
            string data,
            CancellationToken ct)
        {
            // Formato SSE estándar:
            // event: token\n
            // data: el texto aquí\n\n
            var payload = $"event: {eventType}\ndata: {EscapeData(data)}\n\n";
            var bytes = Encoding.UTF8.GetBytes(payload);

            await response.Body.WriteAsync(bytes, ct);
            await response.Body.FlushAsync(ct);
        }

        // SSE no permite saltos de línea dentro del campo data
        // Los reemplazamos por el marcador estándar que el FE reconoce
        private static string EscapeData(string data)
            => data.Replace("\n", "\\n").Replace("\r", string.Empty);
    }
}
