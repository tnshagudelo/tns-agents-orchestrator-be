namespace Domain.Entities
{
    /// <summary>
    /// Estimación completa generada por el ProjectManagerAgent.
    /// Se persiste cuando el agente considera que tiene suficiente contexto.
    /// </summary>
    public class ProjectEstimation
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid SessionId { get; init; }
        public required string ProjectName { get; init; }
        public required string CreatedBy { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// El markdown completo generado por el agente.
        /// Angular lo renderiza directamente con ngx-markdown.
        /// </summary>
        public required string MarkdownContent { get; init; }

        /// <summary>
        /// Resumen ejecutivo para listar estimaciones sin cargar el markdown completo.
        /// </summary>
        public required string Summary { get; init; }

        public EstimationStatus Status { get; set; } = EstimationStatus.Draft;
    }

}
