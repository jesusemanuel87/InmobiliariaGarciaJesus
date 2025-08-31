using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class InmuebleConContrato
    {
        public int Id { get; set; }
        
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";
        
        public TipoInmueble Tipo { get; set; }
        
        public UsoInmueble Uso { get; set; }
        
        public EstadoInmueble Estado { get; set; }
        
        public decimal? Precio { get; set; }
        
        public bool TieneContrato { get; set; }
        
        public int? EstadoContrato { get; set; }
        
        public string EstadoContratoTexto
        {
            get
            {
                if (!TieneContrato)
                    return "Disponible";
                
                return EstadoContrato switch
                {
                    1 => "Vigente",
                    2 => "Finalizado",
                    3 => "Cancelado",
                    _ => "Disponible"
                };
            }
        }
        
        public string EstadoContratoCssClass
        {
            get
            {
                if (!TieneContrato)
                    return "bg-success";
                
                return EstadoContrato switch
                {
                    1 => "bg-primary",
                    2 => "bg-secondary",
                    3 => "bg-danger",
                    _ => "bg-success"
                };
            }
        }
    }
}
