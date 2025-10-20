using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    public enum EstadoPago
    {
        Pendiente,
        Pagado,
        Vencido
    }

    public enum MetodoPago
    {
        MercadoPago,
        Transferencia,
        Efectivo,
        Cheque,
        Otro
    }

    public class Pago
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de pago es obligatorio")]
        [Display(Name = "Número de Pago")]
        public int Numero { get; set; }

        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.Date)]
        public DateTime? FechaPago { get; set; }

        [Required(ErrorMessage = "El contrato es obligatorio")]
        [Display(Name = "Contrato")]
        public int ContratoId { get; set; }

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Display(Name = "Importe")]
        [Range(0.01, 999999999, ErrorMessage = "El importe debe ser mayor a 0")]
        public decimal Importe { get; set; }

        [Display(Name = "Intereses")]
        [Range(0, 999999999, ErrorMessage = "Los intereses no pueden ser negativos")]
        public decimal Intereses { get; set; } = 0;

        [Display(Name = "Multas")]
        [Range(0, 999999999, ErrorMessage = "Las multas no pueden ser negativas")]
        public decimal Multas { get; set; } = 0;

        [Display(Name = "Total a Pagar")]
        public decimal TotalAPagar => Importe + Intereses + Multas;

        [Display(Name = "Fecha de Vencimiento")]
        [DataType(DataType.Date)]
        public DateTime FechaVencimiento { get; set; }

        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Display(Name = "Método de Pago")]
        [Column("metodo_pago")]
        public MetodoPago? MetodoPago { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Campos de auditoría
        [Display(Name = "Creado Por")]
        public int? CreadoPorId { get; set; }

        [Display(Name = "Anulado Por")]
        public int? AnuladoPorId { get; set; }

        [Display(Name = "Fecha de Anulación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaAnulacion { get; set; }

        // Navigation properties para auditoría
        [ForeignKey("CreadoPorId")]
        public virtual Usuario? CreadoPor { get; set; }

        [ForeignKey("AnuladoPorId")]
        public virtual Usuario? AnuladoPor { get; set; }

    }
}
