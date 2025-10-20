using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [Display(Name = "Fecha de Finalización Real")]
        [DataType(DataType.Date)]
        [Column("fecha_finalizacion_real")]
        public DateTime? FechaFinalizacionReal { get; set; }

        [Display(Name = "Multa por Finalización")]
        [Column("multa_finalizacion")]
        public decimal? MultaFinalizacion { get; set; }

        [Display(Name = "Meses Adeudados")]
        [Column("meses_adeudados")]
        public int? MesesAdeudados { get; set; }

        [Display(Name = "Importe Adeudado")]
        [Column("importe_adeudado")]
        public decimal? ImporteAdeudado { get; set; }

        // Campos de auditoría
        [Display(Name = "Creado Por")]
        public int? CreadoPorId { get; set; }

        [Display(Name = "Terminado Por")]
        public int? TerminadoPorId { get; set; }

        [Display(Name = "Fecha de Terminación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaTerminacion { get; set; }

        // Navigation properties
        public Inquilino? Inquilino { get; set; }
        public Inmueble? Inmueble { get; set; }

        [ForeignKey("CreadoPorId")]
        public virtual Usuario? CreadoPor { get; set; }

        [ForeignKey("TerminadoPorId")]
        public virtual Usuario? TerminadoPor { get; set; }

    }
}
