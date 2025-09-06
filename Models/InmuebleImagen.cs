using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    public class InmuebleImagen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Nombre del Archivo")]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Ruta del Archivo")]
        public string RutaArchivo { get; set; } = string.Empty;

        [Display(Name = "Es Portada")]
        public bool EsPortada { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Tamaño (bytes)")]
        public long? TamanoBytes { get; set; }

        [StringLength(100)]
        [Display(Name = "Tipo MIME")]
        public string? TipoMime { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha de Actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // Navegación
        // Propiedades de navegación
        [ForeignKey("InmuebleId")]
        public virtual Inmueble? Inmueble { get; set; }

        // Propiedades calculadas
        public string RutaCompleta => $"/uploads/inmuebles/{InmuebleId}/{NombreArchivo}";
    }
}
