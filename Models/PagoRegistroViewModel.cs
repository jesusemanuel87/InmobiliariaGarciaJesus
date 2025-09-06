using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class PagoRegistroViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Número de Pago")]
        public int Numero { get; set; }
        
        [Display(Name = "Contrato")]
        public int ContratoId { get; set; }
        
        [Display(Name = "Importe")]
        public decimal Importe { get; set; }
        
        [Display(Name = "Intereses")]
        public decimal Intereses { get; set; }
        
        [Display(Name = "Multas")]
        public decimal Multas { get; set; }
        
        [Display(Name = "Total a Pagar")]
        public decimal TotalAPagar { get; set; }
        
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime FechaVencimiento { get; set; }
        
        [Display(Name = "Estado")]
        public EstadoPago Estado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        [Display(Name = "Método de Pago")]
        public MetodoPago MetodoPago { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        // Para mostrar información del contrato
        public string? ContratoInfo { get; set; }
        
        // Información adicional para mostrar en el modal
        public DateTime FechaCreacion { get; set; }
    }
}
