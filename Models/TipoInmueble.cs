using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace InmobiliariaGarciaJesus.Models
{
    public class TipoInmuebleEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del tipo es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        [Display(Name = "Nombre del Tipo")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string? Descripcion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación: Inmuebles que tienen este tipo
        public virtual ICollection<Inmueble>? Inmuebles { get; set; }
    }
}
