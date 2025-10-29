using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    /// <summary>
    /// Cache local de localidades desde API Georef
    /// </summary>
    [Table("georef_localidades")]
    public class GeorefLocalidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string GeorefId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public int ProvinciaId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        public DateTime? FechaActualizacion { get; set; }

        // Navegación
        [ForeignKey("ProvinciaId")]
        public virtual GeorefProvincia? Provincia { get; set; }
    }
}
