using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Application.Shared
{
    /// <summary>
    /// Servicio genérico para agentes multi-turno.
    /// Cualquier agente que necesite mantener historial de conversación
    /// usa este servicio — no implementa su propia lógica de historial.
    ///
    /// Responsabilidades:
    /// 1. Guardar mensaje del usuario
    /// 2. Construir ChatHistory completo para el LLM
    /// 3. Guardar respuesta del agente
    /// 4. Delegar output al handler específico del agente (si tiene uno)
    /// </summary>
    public class ConversationService(
        IConversationRepository conversationRepo,
        IEnumerable<IAgentOutputHandler> outputHandlers,
        ILogger<ConversationService> logger)
    {
        private readonly IConversationRepository _conversationRepo = conversationRepo;
        private readonly IReadOnlyDictionary<AgentType, IAgentOutputHandler> _outputHandlers = outputHandlers.ToDictionary(h => h.AgentType);
        private readonly ILogger<ConversationService> _logger = logger;

        /// <summary>
        /// Guarda el mensaje del usuario y devuelve el ChatHistory
        /// completo listo para pasarle al LLM.
        /// </summary>
        public async Task<ChatHistory> PrepareAsync(
            AgentRequest request,
            Guid sessionId,
            string systemPrompt,
            CancellationToken ct)
        {
            // Guarda el mensaje del usuario
            await _conversationRepo.SaveMessageAsync(new ConversationMessage
            {
                SessionId = sessionId,
                Role = "user",
                Content = request.UserMessage
            }, ct);

            // Construye el ChatHistory con todo el historial
            var history = new ChatHistory(systemPrompt);

            var previousMessages = await _conversationRepo.GetHistoryAsync(sessionId, ct);

            // Excluye el último mensaje porque ya lo agregamos arriba
            // y lo agregaremos al final para mantener el orden correcto
            var historyWithoutLast = previousMessages
                .OrderBy(m => m.Timestamp)
                .SkipLast(1)
                .ToList();

            foreach (var msg in historyWithoutLast)
            {
                if (msg.Role == "user")
                    history.AddUserMessage(msg.Content);
                else
                    history.AddAssistantMessage(msg.Content);
            }

            // Agrega el mensaje actual al final
            history.AddUserMessage(request.UserMessage);

            _logger.LogInformation(
                "[ConversationService] ChatHistory construido. SessionId={SessionId} Mensajes={Count}",
                sessionId, history.Count
            );

            return history;
        }

        /// <summary>
        /// Guarda la respuesta del agente y dispara el output handler
        /// específico si existe para este tipo de agente.
        /// </summary>
        public async Task CommitAsync(
            AgentRequest request,
            Guid sessionId,
            string agentResponse,
            CancellationToken ct)
        {
            // Guarda la respuesta del agente
            await _conversationRepo.SaveMessageAsync(new ConversationMessage
            {
                SessionId = sessionId,
                Role = "assistant",
                Content = agentResponse
            }, ct);

            // Dispara el handler específico si este agente tiene uno
            if (_outputHandlers.TryGetValue(request.TargetAgent, out var handler))
            {
                _logger.LogInformation(
                    "[ConversationService] Disparando OutputHandler para {Agent}",
                    request.TargetAgent
                );

                await handler.HandleAsync(request, sessionId, agentResponse, ct);
            }
        }

        /// <summary>
        /// Extrae y valida el sessionId del Metadata del request.
        /// Centralizado aquí para que todos los agentes multi-turno
        /// lo hagan igual.
        /// </summary>
        public static Guid ExtractSessionId(AgentRequest request)
        {
            if (!request.Metadata.TryGetValue("sessionId", out var sessionIdStr)
                || !Guid.TryParse(sessionIdStr, out var sessionId))
            {
                throw new ArgumentException(
                    "Metadata debe incluir 'sessionId' como GUID válido."
                );
            }

            return sessionId;
        }
    }
}
