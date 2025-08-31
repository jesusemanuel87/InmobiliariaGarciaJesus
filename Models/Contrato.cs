using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public enum EstadoContrato
    {
        Reservado,
        Activo,
        Finalizado,
        Cancelado
    }

    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Display(Name = "Precio Mensual")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [Display(Name = "Estado")]
        public EstadoContrato Estado { get; set; } = EstadoContrato.Reservado;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Motivo de Cancelación")]
        public string? MotivoCancelacion { get; set; }

        // Navigation properties
        public Inquilino? Inquilino { get; set; }
        public Inmueble? Inmueble { get; set; }

    }
}
