using Domain.Entities;
using Domain.Interfaces;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection;

namespace Application.Agents.ProjectManagerAgent
{
    /// <summary>
    /// Construye el historial de chat para el ProjectManagerAgent.
    /// La clave del multi-turno: cada llamada al LLM incluye
    /// todos los mensajes anteriores de la sesión.
    /// </summary>
    public class ProjectManagerContextBuilder
    {
        private readonly IConversationRepository _conversationRepo;

        public ProjectManagerContextBuilder(IConversationRepository conversationRepo)
        {
            _conversationRepo = conversationRepo;
        }

        /// <summary>
        /// Construye el ChatHistory completo para esta sesión.
        /// SK usa esto para que el LLM tenga contexto de toda la conversación.
        /// </summary>
        public async Task<ChatHistory> BuildChatHistoryAsync(
            AgentRequest request,
            Guid sessionId,
            CancellationToken ct)
        {
            var systemPrompt = BuildSystemPrompt(request.UserName, sessionId);
            var history = new ChatHistory(systemPrompt);

            // Carga todos los mensajes anteriores de esta sesión
            var previousMessages = await _conversationRepo.GetHistoryAsync(sessionId, ct);

            foreach (var msg in previousMessages.OrderBy(m => m.Timestamp))
            {
                if (msg.Role == "user")
                    history.AddUserMessage(msg.Content);
                else
                    history.AddAssistantMessage(msg.Content);
            }

            // Agrega el mensaje actual del usuario
            history.AddUserMessage(request.UserMessage);

            return history;
        }

        private string BuildSystemPrompt(string userName, Guid sessionId)
        {
            var template = LoadPromptTemplate();
            return template
                .Replace("{{userName}}", userName)
                .Replace("{{sessionId}}", sessionId.ToString());
        }

        private static string LoadPromptTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Application.Agents.ProjectManagerAgent.Prompts.SystemPrompt.txt";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"No se encontró: {resourceName}");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
