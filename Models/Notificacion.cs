using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    /// <summary>
    /// Tipos de notificaciones para la app móvil
    /// </summary>
    public enum TipoNotificacion
    {
        PagoRegistrado,
        PagoVencido,
        ProximoVencimiento,
        NuevoContrato,
        ContratoFinalizado,
        NuevoInquilino
    }

    /// <summary>
    /// Notificaciones in-app para propietarios
    /// </summary>
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PropietarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Datos adicionales en formato JSON (pagoId, contratoId, etc.)
        /// </summary>
        public string? Datos { get; set; }

        public bool Leida { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaLeida { get; set; }

        // Navegación
        [ForeignKey("PropietarioId")]
        public virtual Propietario? Propietario { get; set; }
    }
}
