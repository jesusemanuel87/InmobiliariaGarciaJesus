using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class PagoEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Pago")]
        public int Numero { get; set; }
        
        [Display(Name = "Contrato")]
        public int ContratoId { get; set; }
        
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }
        
        [Display(Name = "Fecha de Pago")]
        public DateTime? FechaPago { get; set; }

        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; }

        [Display(Name = "Método de Pago")]
        public MetodoPago? MetodoPago { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; }

        // Para mostrar información del contrato
        public string? ContratoInfo { get; set; }
    }
}
