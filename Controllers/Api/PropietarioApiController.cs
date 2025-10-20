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

        public PropietarioApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<PropietarioApiController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Obtener perfil del propietario autenticado
        /// </summary>
        [HttpGet("perfil")]
        [ProducesResponseType(typeof(ApiResponse<PropietarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PropietarioDto>>> ObtenerPerfil()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Id == propietarioId.Value);

                if (propietario == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Propietario no encontrado"));
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

                return Ok(ApiResponse<PropietarioDto>.SuccessResponse(propietarioDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del propietario");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Actualizar perfil del propietario autenticado
        /// </summary>
        [HttpPut("perfil")]
        [ProducesResponseType(typeof(ApiResponse<PropietarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PropietarioDto>>> ActualizarPerfil([FromBody] ActualizarPerfilDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Datos de entrada inválidos", errors));
                }

                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var propietario = await _context.Propietarios
                    .FirstOrDefaultAsync(p => p.Id == propietarioId.Value);

                if (propietario == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Propietario no encontrado"));
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
                return Ok(ApiResponse<PropietarioDto>.SuccessResponse(propietarioDto, "Perfil actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del propietario");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Subir foto de perfil
        /// </summary>
        [HttpPost("perfil/foto")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<string>>> SubirFotoPerfil([FromForm] IFormFile foto)
        {
            try
            {
                if (foto == null || foto.Length == 0)
                {
                    return BadRequest(ApiResponse.ErrorResponse("No se recibió ninguna imagen"));
                }

                // Validar tamaño (máx 5MB)
                if (foto.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(ApiResponse.ErrorResponse("La imagen no puede superar 5MB"));
                }

                // Validar extensión
                var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!extensionesPermitidas.Contains(extension))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Formato de imagen no permitido. Use: jpg, jpeg, png, gif o webp"));
                }

                var usuarioId = _jwtService.ObtenerUsuarioId(User);
                if (usuarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var usuario = await _context.Usuarios.FindAsync(usuarioId.Value);
                if (usuario == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Usuario no encontrado"));
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
                return Ok(ApiResponse<string>.SuccessResponse(usuario.FotoPerfil, "Foto de perfil actualizada exitosamente"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto de perfil");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }
    }
}
