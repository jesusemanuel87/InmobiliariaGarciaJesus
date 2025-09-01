using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class ContratoFinalizacionViewModel
    {
        public int ContratoId { get; set; }
        public Contrato? Contrato { get; set; }
        
        [Display(Name = "Fecha de Finalización")]
        [DataType(DataType.Date)]
        public DateTime FechaFinalizacion { get; set; } = DateTime.Today;

        [Display(Name = "Motivo")]
        [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string? Motivo { get; set; }

        // Información calculada para finalización
        public decimal? MultaCalculada { get; set; }
        public int MesesAdeudados { get; set; }
        public decimal ImporteAdeudado { get; set; }
        public bool EsFinalizacionTemprana { get; set; }
        public int MesesCumplidos { get; set; }
        public int MesesTotales { get; set; }
        public List<Pago> PagosAtrasados { get; set; } = new List<Pago>();
    }
}
