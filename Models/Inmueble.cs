using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace InmobiliariaGarciaJesus.Models
{
    public enum TipoInmueble
    {
        Casa,
        Departamento,
        Monoambiente,
        Local,
        Oficina,
        Terreno,
        [Display(Name = "Galpón")]
        Galpon
    }

    public enum UsoInmueble
    {
        Residencial,
        Comercial,
        Industrial
    }

    public enum EstadoInmueble
    {
        Activo,
        Inactivo
    }

    public enum DisponibilidadInmueble
    {
        Disponible,
        Reservado,
        [Display(Name = "No Disponible")]
        NoDisponible
    }

    public class Inmueble
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Localidad")]
        public string? Localidad { get; set; }

        [StringLength(100)]
        [Display(Name = "Provincia")]
        public string? Provincia { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public int TipoId { get; set; } = 1; // Default: Casa

        [Required]
        [Display(Name = "Ambientes")]
        public int Ambientes { get; set; }

        [Display(Name = "Superficie")]
        public decimal? Superficie { get; set; }

        [Display(Name = "Latitud")]
        public decimal? Latitud { get; set; }

        [Display(Name = "Longitud")]
        public decimal? Longitud { get; set; }

        [Required]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [Display(Name = "Disponible")]
        [NotMapped] // Esta columna no existe en la BD, se calcula desde contratos
        public bool Disponible { get; set; } = true;

        [Display(Name = "Precio")]
        [Range(0, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Precio { get; set; }

        [Display(Name = "Estado")]
        public EstadoInmueble Estado { get; set; } = EstadoInmueble.Activo;

        [Required(ErrorMessage = "El uso del inmueble es obligatorio")]
        [Display(Name = "Uso")]
        public UsoInmueble Uso { get; set; } = UsoInmueble.Residencial;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("PropietarioId")]
        public virtual Propietario? Propietario { get; set; }

        [ForeignKey("TipoId")]
        public virtual TipoInmuebleEntity? TipoInmueble { get; set; }

        // Propiedad computada para compatibilidad con vistas antiguas
        [NotMapped]
        public dynamic? Tipo => TipoInmueble != null ? (dynamic)new { Nombre = TipoInmueble.Nombre } : null;

        public virtual ICollection<Contrato>? Contratos { get; set; }
        
        public virtual ICollection<InmuebleImagen>? Imagenes { get; set; }

        // Propiedad calculada para obtener la imagen de portada
        [NotMapped]
        public InmuebleImagen? ImagenPortada => Imagenes?.FirstOrDefault(i => i.EsPortada);

        // Propiedad calculada para obtener la URL de la imagen de portada
        [NotMapped]
        public string? ImagenPortadaUrl => ImagenPortada?.RutaCompleta;

        // Propiedad calculada para obtener el enlace de Google Maps
        [NotMapped]
        public string? GoogleMapsUrl => 
            Latitud.HasValue && Longitud.HasValue 
                ? $"https://www.google.com/maps?q={Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                : null;

    }
}
