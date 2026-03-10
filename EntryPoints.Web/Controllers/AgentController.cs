using Application.Orchestration;
using EntryPoints.Web.Mappers;
using EntryPoints.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EntryPoints.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]  // requiere JWT válido — configurado en Program.cs
    public class AgentController(CoreDispatcher dispatcher, ILogger<AgentController> logger) : ControllerBase
    {
        private readonly CoreDispatcher _dispatcher = dispatcher;
        private readonly ILogger<AgentController> _logger = logger;

        /// <summary>
        /// Ejecuta un agente con la petición del usuario.
        /// El frontend Angular llama este endpoint.
        /// </summary>
        [HttpPost("run")]
        [ProducesResponseType(typeof(AgentHttpResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AgentHttpResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RunAsync(
            [FromBody] AgentHttpRequest request,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extrae el usuario del JWT — viene del portal Angular via Entra ID / B2C
            var userName = User.FindFirstValue(ClaimTypes.Name)
                        ?? User.FindFirstValue("preferred_username")
                        ?? "unknown";

            _logger.LogInformation(
                "[AgentController] POST /run Agent={Agent} User={User}",
                request.Agent, userName
            );

            try
            {
                var agentRequest = RequestNormalizer.ToAgentRequest(request, userName);
                var result = await _dispatcher.DispatchAsync(agentRequest, ct);
                var response = RequestNormalizer.ToHttpResponse(result);

                return result.Success
                    ? Ok(response)
                    : BadRequest(response);
            }
            catch (ArgumentException ex)
            {
                // Agente desconocido u otros errores de validación
                _logger.LogWarning("[AgentController] Request inválido: {Error}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Healthcheck — el portal Angular puede llamar esto para
        /// verificar que la API está viva antes de mostrar el chat.
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
            => Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }
}
