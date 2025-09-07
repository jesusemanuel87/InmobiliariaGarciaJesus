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

    public class Inmueble
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo")]
        public TipoInmueble Tipo { get; set; } = TipoInmueble.Casa;

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

        public virtual ICollection<Contrato>? Contratos { get; set; }
        
        public virtual ICollection<InmuebleImagen>? Imagenes { get; set; }

        // Propiedad calculada para obtener la imagen de portada
        [NotMapped]
        public InmuebleImagen? ImagenPortada => Imagenes?.FirstOrDefault(i => i.EsPortada);

        // Propiedad calculada para obtener la URL de la imagen de portada
        [NotMapped]
        public string? ImagenPortadaUrl => ImagenPortada?.RutaCompleta;

    }
}
