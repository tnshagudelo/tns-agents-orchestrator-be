using Domain.Entities;

namespace Domain.Interfaces
{
    /// <summary>
    /// Persiste y recupera el historial de conversación por sesión.
    /// Crítico para el multi-turno — sin esto el agente pierde contexto.
    /// </summary>
    public interface IConversationRepository
    {
        Task SaveMessageAsync(ConversationMessage message, CancellationToken ct = default);
        Task<IEnumerable<ConversationMessage>> GetHistoryAsync(Guid sessionId, CancellationToken ct = default);
        Task ClearSessionAsync(Guid sessionId, CancellationToken ct = default);

        Task<bool> SessionExistsAsync(Guid sessionId, CancellationToken ct = default);
    }
}
