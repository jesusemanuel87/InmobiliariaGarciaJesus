using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(255, ErrorMessage = "La clave no puede exceder los 255 caracteres")]
        public string ClaveHash { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "La ruta de la foto no puede exceder los 255 caracteres")]
        [Display(Name = "Foto de Perfil")]
        public string? FotoPerfil { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public RolUsuario Rol { get; set; }

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; } = true;

        [Display(Name = "Requiere Cambio de Clave")]
        public bool RequiereCambioClave { get; set; } = false;

        // Propiedad de compatibilidad para Password (mapea a ClaveHash)
        [NotMapped]
        public string Password
        {
            get => ClaveHash;
            set => ClaveHash = value;
        }

        // Foreign Keys (solo uno será válido según el rol)
        public int? EmpleadoId { get; set; }
        public int? PropietarioId { get; set; }
        public int? InquilinoId { get; set; }

        // Navigation Properties
        [ForeignKey("EmpleadoId")]
        public virtual Empleado? Empleado { get; set; }

        [ForeignKey("PropietarioId")]
        public virtual Propietario? Propietario { get; set; }

        [ForeignKey("InquilinoId")]
        public virtual Inquilino? Inquilino { get; set; }

        // Propiedades calculadas
        [NotMapped]
        public string NombreCompleto
        {
            get
            {
                return Rol switch
                {
                    RolUsuario.Empleado => Empleado?.NombreCompleto ?? "Usuario",
                    RolUsuario.Administrador => Empleado?.NombreCompleto ?? "Administrador",
                    RolUsuario.Propietario => Propietario?.NombreCompleto ?? "Propietario",
                    RolUsuario.Inquilino => Inquilino?.NombreCompleto ?? "Inquilino",
                    _ => "Usuario"
                };
            }
        }

        [NotMapped]
        public string RolDescripcion
        {
            get
            {
                return Rol switch
                {
                    RolUsuario.Empleado => "Empleado",
                    RolUsuario.Administrador => "Administrador",
                    RolUsuario.Propietario => "Propietario",
                    RolUsuario.Inquilino => "Inquilino",
                    _ => "Sin Rol"
                };
            }
        }

        [NotMapped]
        public bool EsAdministrador => Rol == RolUsuario.Administrador;

        [NotMapped]
        public bool EsEmpleado => Rol == RolUsuario.Empleado || Rol == RolUsuario.Administrador;

        [NotMapped]
        public bool EsPropietario => Rol == RolUsuario.Propietario;

        [NotMapped]
        public bool EsInquilino => Rol == RolUsuario.Inquilino;
    }

    public enum RolUsuario
    {
        Propietario = 1,
        Inquilino = 2,
        Empleado = 3,
        Administrador = 4
    }
}
