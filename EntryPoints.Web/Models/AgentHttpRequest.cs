using System.ComponentModel.DataAnnotations;

namespace EntryPoints.Web.Models
{
    /// <summary>
    /// Body del POST que envía el portal Angular.
    /// Contrato público de la API — si cambia, el frontend lo siente.
    /// </summary>
    public class AgentHttpRequest
    {
        /// <summary>
        /// Lo que el usuario escribió en el chat.
        /// Ej: "genera tests para OrderService.cs"
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public required string Message { get; init; }

        /// <summary>
        /// Qué agente debe procesar esto.
        /// Por ahora solo "UnitTestAgent" disponible.
        /// </summary>
        [Required]
        public required string Agent { get; init; }

        /// <summary>
        /// Datos adicionales que el agente necesita.
        /// El frontend los envía según qué agente se seleccionó.
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new();
    }
}
