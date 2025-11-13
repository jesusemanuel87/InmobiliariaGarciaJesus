using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Controllers.Api
{
    /// <summary>
    /// API Controller para gestión de perfil de propietario
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class PropietarioApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<PropietarioApiController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly UsuarioService _usuarioService;

        public PropietarioApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<PropietarioApiController> logger,
            IWebHostEnvironment environment,
            UsuarioService usuarioService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _environment = environment;
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtener perfil del propietario autenticado
        /// </summary>
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(PropietarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropietarioDto>> ObtenerPerfil()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Id == propietarioId.Value);

                if (propietario == null)
                {
                    return NotFound(new { error = "Propietario no encontrado" });
                }

                // Obtener foto de perfil del usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PropietarioId == propietarioId.Value);

                var propietarioDto = new PropietarioDto
                {
                    Id = propietario.Id,
                    Nombre = propietario.Nombre,
                    Apellido = propietario.Apellido,
                    NombreCompleto = propietario.NombreCompleto,
                    Dni = propietario.Dni,
                    Telefono = propietario.Telefono,
                    Email = propietario.Email,
                    Direccion = propietario.Direccion,
                    Estado = propietario.Estado,
                    FotoPerfil = usuario?.FotoPerfil
                };

                return Ok(propietarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del propietario");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar perfil del propietario autenticado
        /// </summary>
        [HttpPut("perfil")]
        [ProducesResponseType(typeof(PropietarioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PropietarioDto>> ActualizarPerfil([FromBody] ActualizarPerfilDto request)
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

                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Id == propietarioId.Value);

                if (propietario == null)
                {
                    return NotFound(new { error = "Propietario no encontrado" });
                }

                // Actualizar datos
                propietario.Nombre = request.Nombre;
                propietario.Apellido = request.Apellido;
                propietario.Telefono = request.Telefono;
                propietario.Direccion = request.Direccion;

                await _context.SaveChangesAsync();

                // Obtener foto de perfil del usuario
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.PropietarioId == propietarioId.Value);

                var propietarioDto = new PropietarioDto
                {
                    Id = propietario.Id,
                    Nombre = propietario.Nombre,
                    Apellido = propietario.Apellido,
                    NombreCompleto = propietario.NombreCompleto,
                    Dni = propietario.Dni,
                    Telefono = propietario.Telefono,
                    Email = propietario.Email,
                    Direccion = propietario.Direccion,
                    Estado = propietario.Estado,
                    FotoPerfil = usuario?.FotoPerfil
                };

                _logger.LogInformation($"Perfil actualizado para propietario ID: {propietarioId}");
                return Ok(propietarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del propietario");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Subir foto de perfil
        /// </summary>
        [HttpPost("perfil/foto")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> SubirFotoPerfil([FromForm] IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    return BadRequest(new { error = "No se recibió ninguna imagen" });
                }

                // Validar tamaño (máx 5MB)
                if (foto.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { error = "La imagen no puede superar 5MB" });
                }

                // Validar extensión
                var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!extensionesPermitidas.Contains(extension))
                {
                    return BadRequest(new { error = "Formato de imagen no permitido. Use: jpg, jpeg, png, gif o webp" });
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

                // Crear directorio si no existe
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "perfiles");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Eliminar foto anterior si existe
                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var fotoAnterior = Path.Combine(_environment.WebRootPath, usuario.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(fotoAnterior))
                    {
                        System.IO.File.Delete(fotoAnterior);
                    }
                }

                // Guardar nueva foto
                var nombreArchivo = $"{usuarioId}_{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                // Actualizar base de datos
                usuario.FotoPerfil = $"/uploads/perfiles/{nombreArchivo}";
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Foto de perfil actualizada para usuario ID: {usuarioId}");
                return Ok(usuario.FotoPerfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto de perfil");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambiar contraseña del propietario autenticado según especificación Android
        /// Método: PUT
        /// Ruta: /api/Propietarios/changePassword
        /// Tipo: application/x-www-form-urlencoded
        /// </summary>
        [HttpPut("/api/Propietarios/changePassword")]
        [Consumes("application/x-www-form-urlencoded")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromForm] string currentPassword, [FromForm] string newPassword)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    return BadRequest(new { error = "La contraseña actual es obligatoria" });
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    return BadRequest(new { error = "La nueva contraseña es obligatoria" });
                }

                if (newPassword.Length < 6)
                {
                    return BadRequest(new { error = "La contraseña debe tener al menos 6 caracteres" });
                }

                if (currentPassword == newPassword)
                {
                    return BadRequest(new { error = "La nueva contraseña debe ser diferente a la actual" });
                }

                // Obtener usuario autenticado
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
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, usuario.Password))
                {
                    return BadRequest(new { error = "La contraseña actual es incorrecta" });
                }

                // Actualizar contraseña
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Contraseña cambiada para usuario ID: {usuarioId}");
                
                // Respuesta void (solo status 200 OK)
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}
