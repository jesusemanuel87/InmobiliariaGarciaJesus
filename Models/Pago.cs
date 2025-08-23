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
        [Column(TypeName = "decimal(15,2)")]
        [Range(0.01, 999999999, ErrorMessage = "El importe debe ser mayor a 0")]
        public decimal Importe { get; set; }

        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("ContratoId")]
        public virtual Contrato? Contrato { get; set; }
    }
}
