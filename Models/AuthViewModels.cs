using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario")]
        [Display(Name = "Tipo de Usuario")]
        public RolUsuario TipoUsuario { get; set; }

        // Campos para Propietario/Inquilino
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [Display(Name = "DNI")]
        public string? Dni { get; set; }

        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RolUsuario Rol { get; set; }
        public string? FotoPerfil { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        // Información de la persona asociada
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Dni { get; set; }
        public string? Telefono { get; set; }

        // Para empleados
        public RolEmpleado? RolEmpleado { get; set; }
        public DateTime? FechaIngreso { get; set; }

        // Lista de roles disponibles para el usuario
        public List<RolUsuario> RolesDisponibles { get; set; } = new();
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nueva Contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class SwitchRoleViewModel
    {
        public int UsuarioId { get; set; }
        public RolUsuario RolActual { get; set; }
        public RolUsuario NuevoRol { get; set; }
        public List<RolUsuario> RolesDisponibles { get; set; } = new();
        public string NombreCompleto { get; set; } = string.Empty;
    }
}
