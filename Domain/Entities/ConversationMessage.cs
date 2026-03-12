namespace Domain.Entities
{
    /// <summary>
    /// Un mensaje dentro de una conversación con el agente.
    /// Se guarda en BD para mantener el historial entre turnos.
    /// </summary>
    public class ConversationMessage
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid SessionId { get; init; }
        public required string Role { get; init; }    // "user" o "assistant"
        public required string Content { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
