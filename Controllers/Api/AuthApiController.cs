using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Services;
using BCrypt.Net;

namespace InmobiliariaGarciaJesus.Controllers.Api
{
    /// <summary>
    /// API Controller para autenticación de propietarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<AuthApiController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Login de propietarios
        /// </summary>
        /// <param name="request">Credenciales de login</param>
        /// <returns>Token JWT y datos del propietario</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Datos de entrada inválidos", details = errors });
                }

                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.Propietario)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de login fallido: Email {request.Email} no encontrado");
                    return Unauthorized(new { error = "Credenciales inválidas" });
                }

                // Verificar que sea un propietario
                if (usuario.Rol != RolUsuario.Propietario || usuario.Propietario == null)
                {
                    _logger.LogWarning($"Intento de login de usuario no propietario: {request.Email}");
                    return Unauthorized(new { error = "Acceso no autorizado. Solo propietarios pueden usar esta aplicación" });
                }

                // Verificar contraseña
                if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.ClaveHash))
                {
                    _logger.LogWarning($"Intento de login fallido: Contraseña incorrecta para {request.Email}");
                    return Unauthorized(new { error = "Credenciales inválidas" });
                }

                // Verificar que la cuenta esté activa
                if (!usuario.Estado)
                {
                    _logger.LogWarning($"Intento de login de cuenta inactiva: {request.Email}");
                    return Unauthorized(new { error = "Cuenta inactiva. Contacte al administrador" });
                }

                // Actualizar último acceso
                usuario.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                // Generar token JWT
                var token = _jwtService.GenerarToken(usuario, usuario.Propietario);
                var expiracion = DateTime.UtcNow.AddMinutes(1440); // 24 horas

                var propietarioDto = new PropietarioDto
                {
                    Id = usuario.Propietario.Id,
                    Nombre = usuario.Propietario.Nombre,
                    Apellido = usuario.Propietario.Apellido,
                    NombreCompleto = usuario.Propietario.NombreCompleto,
                    Dni = usuario.Propietario.Dni,
                    Telefono = usuario.Propietario.Telefono,
                    Email = usuario.Propietario.Email,
                    Direccion = usuario.Propietario.Direccion,
                    Estado = usuario.Propietario.Estado,
                    FotoPerfil = usuario.FotoPerfil
                };

                var response = new LoginResponseDto
                {
                    Token = token,
                    Propietario = propietarioDto,
                    Expiracion = expiracion
                };

                _logger.LogInformation($"Login exitoso para propietario: {usuario.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Login de propietarios (formato form-urlencoded)
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Token JWT y datos del propietario</returns>
        [HttpPost("login-form")]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseDto>> LoginForm([FromForm] string email, [FromForm] string password)
        {
            // Crear objeto LoginRequestDto y reutilizar el método Login existente
            var request = new LoginRequestDto
            {
                Email = email,
                Password = password
            };

            // Reutilizar la lógica del método Login
            return await Login(request);
        }

        /// <summary>
        /// Cambiar contraseña del propietario autenticado
        /// </summary>
        [HttpPost("cambiar-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> CambiarPassword([FromBody] CambiarPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Datos de entrada inválidos", errors });
                }

                var usuarioId = _jwtService.ObtenerUsuarioId(User);
                if (usuarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                var usuario = await _context.Usuarios.FindAsync(usuarioId.Value);
                if (usuario == null)
                {
                    return NotFound(new { error = "Usuario no encontrado" });
                }

                // Verificar contraseña actual
                if (!BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.ClaveHash))
                {
                    return BadRequest(new { error = "La contraseña actual es incorrecta" });
                }

                // Actualizar contraseña
                usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNueva);
                usuario.RequiereCambioClave = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña cambiada exitosamente para usuario ID: {usuarioId}");
                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Resetear contraseña (olvidé mi contraseña)
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Datos de entrada inválidos", errors });
                }

                // Buscar propietario por email y DNI
                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Email == request.Email && p.Dni == request.Dni);

                if (propietario == null)
                {
                    _logger.LogWarning($"Intento de reset de contraseña fallido: Email {request.Email} y DNI {request.Dni} no coinciden");
                    return Ok(new { message = "Si el email existe, se enviará un correo con instrucciones para restablecer la contraseña" });
                }

                // Buscar usuario asociado
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PropietarioId == propietario.Id && u.Rol == RolUsuario.Propietario);

                if (usuario == null)
                {
                    return NotFound(new { error = "No se encontró una cuenta de usuario asociada" });
                }

                // Generar nueva contraseña temporal (8 caracteres aleatorios)
                var nuevaPassword = GenerarPasswordTemporal();
                usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                usuario.RequiereCambioClave = true; // Forzar cambio de contraseña en próximo login
                await _context.SaveChangesAsync();

                var responseData = new ResetPasswordResponseDto
                {
                    Success = true,
                    Message = "Contraseña restablecida exitosamente",
                    NuevaPassword = nuevaPassword
                };

                _logger.LogInformation($"Contraseña restablecida para usuario: {usuario.Email}");
                
                // NOTA: En producción, esto debería enviarse por email, no retornarse en la respuesta
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al resetear contraseña");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Genera una contraseña temporal aleatoria
        /// </summary>
        private string GenerarPasswordTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
