using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    /// <summary>
    /// Cache local de provincias desde API Georef
    /// </summary>
    [Table("georef_provincias")]
    public class GeorefProvincia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string GeorefId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        public virtual ICollection<GeorefLocalidad> Localidades { get; set; } = new List<GeorefLocalidad>();
    }
}
