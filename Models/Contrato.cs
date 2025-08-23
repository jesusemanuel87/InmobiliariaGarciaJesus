using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    public enum EstadoContrato
    {
        Activo,
        Finalizado,
        Cancelado
    }

    public class Contrato
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Display(Name = "Precio Mensual")]
        [Column(TypeName = "decimal(15,2)")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El inquilino es obligatorio")]
        [Display(Name = "Inquilino")]
        public int InquilinoId { get; set; }

        [Required(ErrorMessage = "El inmueble es obligatorio")]
        [Display(Name = "Inmueble")]
        public int InmuebleId { get; set; }

        [Display(Name = "Estado")]
        public EstadoContrato Estado { get; set; } = EstadoContrato.Activo;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Navegación
        [ForeignKey("InquilinoId")]
        public virtual Inquilino? Inquilino { get; set; }

        [ForeignKey("InmuebleId")]
        public virtual Inmueble? Inmueble { get; set; }

        public virtual ICollection<Pago>? Pagos { get; set; }
    }
}
