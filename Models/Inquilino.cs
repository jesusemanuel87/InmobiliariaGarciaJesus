using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class Inquilino : Persona
    {
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;
    }
}
