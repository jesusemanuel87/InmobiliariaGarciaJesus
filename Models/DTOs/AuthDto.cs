using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models.DTOs
{
    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para respuesta de login exitoso
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public PropietarioDto Propietario { get; set; } = null!;
        public DateTime Expiracion { get; set; }
    }

    /// <summary>
    /// DTO para solicitud de cambio de contraseña
    /// </summary>
    public class CambiarPasswordRequestDto
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        public string PasswordActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string PasswordNueva { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria")]
        [Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden")]
        public string PasswordConfirmacion { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitud de reseteo de contraseña
    /// </summary>
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        public string Dni { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para respuesta de reseteo de contraseña
    /// </summary>
    public class ResetPasswordResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NuevaPassword { get; set; }
    }
}
