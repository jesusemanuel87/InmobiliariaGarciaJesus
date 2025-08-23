using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    public enum TipoInmueble
    {
        Casa,
        Departamento,
        Local,
        Oficina,
        Terreno
    }

    public enum UsoInmueble
    {
        Residencial,
        Comercial,
        Industrial
    }

    public class Inmueble
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";

        [Required(ErrorMessage = "El número de ambientes es obligatorio")]
        [Range(1, 50, ErrorMessage = "Los ambientes deben estar entre 1 y 50")]
        [Display(Name = "Ambientes")]
        public int Ambientes { get; set; }

        [Required(ErrorMessage = "La superficie es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La superficie debe estar entre 1 y 10000 m²")]
        [Display(Name = "Superficie (m²)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Superficie { get; set; }

        [Display(Name = "Latitud")]
        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitud { get; set; }

        [Display(Name = "Longitud")]
        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitud { get; set; }

        [Required(ErrorMessage = "El propietario es obligatorio")]
        [Display(Name = "Propietario")]
        public int PropietarioId { get; set; }

        [Required(ErrorMessage = "El tipo de inmueble es obligatorio")]
        [Display(Name = "Tipo")]
        public TipoInmueble Tipo { get; set; }

        [Display(Name = "Precio")]
        [Column(TypeName = "decimal(15,2)")]
        [Range(0, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? Precio { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Required(ErrorMessage = "El uso del inmueble es obligatorio")]
        [Display(Name = "Uso")]
        public UsoInmueble Uso { get; set; } = UsoInmueble.Residencial;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("PropietarioId")]
        public virtual Propietario? Propietario { get; set; }

        public virtual ICollection<Contrato>? Contratos { get; set; }
    }
}
