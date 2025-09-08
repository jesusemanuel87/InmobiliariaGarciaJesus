using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class Empleado : Persona
    {
        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        public RolEmpleado Rol { get; set; }

        [Display(Name = "Fecha de Ingreso")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [StringLength(500, ErrorMessage = "La ruta de la foto no puede exceder los 500 caracteres")]
        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        // Propiedades adicionales para la entidad
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
        public bool Estado { get; set; } = true;

        // Propiedad para mostrar el rol como string
        public string RolDescripcion => Rol == RolEmpleado.Administrador ? "Administrador" : "Empleado";
    }

    public enum RolEmpleado
    {
        Empleado = 1,
        Administrador = 2
    }
}
