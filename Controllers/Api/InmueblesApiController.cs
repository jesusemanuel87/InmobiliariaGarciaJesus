using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Controllers.Api
{
    /// <summary>
    /// API Controller para gestión de inmuebles del propietario
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class InmueblesApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<InmueblesApiController> _logger;
        private readonly IWebHostEnvironment _environment;

        public InmueblesApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<InmueblesApiController> logger,
            IWebHostEnvironment environment)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Listar inmuebles del propietario autenticado
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<InmuebleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<InmuebleDto>>>> ListarInmuebles()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var inmuebles = await _context.Inmuebles
                    .Include(i => i.TipoInmueble)
                    .Where(i => i.PropietarioId == propietarioId.Value)
                    .OrderByDescending(i => i.FechaCreacion)
                    .ToListAsync();
                
                // Cargar colecciones por separado (EF Core no permite múltiples Include de colecciones)
                foreach (var inmueble in inmuebles)
                {
                    await _context.Entry(inmueble).Collection(i => i.Imagenes!).LoadAsync();
                    await _context.Entry(inmueble).Collection(i => i.Contratos!).LoadAsync();
                }

                var inmueblesDto = inmuebles.Select(i => MapearInmuebleADto(i)).ToList();

                return Ok(ApiResponse<List<InmuebleDto>>.SuccessResponse(inmueblesDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar inmuebles");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtener detalle de un inmueble específico
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<InmuebleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<InmuebleDto>>> ObtenerInmueble(int id)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var inmueble = await _context.Inmuebles
                    .Include(i => i.TipoInmueble)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inmueble == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Inmueble no encontrado"));
                }

                // Verificar que el inmueble pertenece al propietario autenticado
                if (inmueble.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, ApiResponse.ErrorResponse("No tiene permiso para acceder a este inmueble"));
                }

                // Cargar colecciones por separado
                await _context.Entry(inmueble).Collection(i => i.Imagenes!).LoadAsync();
                await _context.Entry(inmueble).Collection(i => i.Contratos!).LoadAsync();

                var inmuebleDto = MapearInmuebleADto(inmueble);

                return Ok(ApiResponse<InmuebleDto>.SuccessResponse(inmuebleDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener inmueble ID: {id}");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Crear un nuevo inmueble (por defecto deshabilitado según requisitos)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InmuebleDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<InmuebleDto>>> CrearInmueble([FromBody] CrearInmuebleDto request)
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

                // Crear inmueble (por defecto inactivo según requisitos)
                var inmueble = new Inmueble
                {
                    Direccion = request.Direccion,
                    Localidad = request.Localidad,
                    Provincia = request.Provincia,
                    TipoId = request.TipoId,
                    Ambientes = request.Ambientes,
                    Superficie = request.Superficie,
                    Latitud = request.Latitud,
                    Longitud = request.Longitud,
                    Precio = request.Precio,
                    Uso = (UsoInmueble)request.Uso,
                    PropietarioId = propietarioId.Value,
                    Estado = EstadoInmueble.Inactivo, // Por defecto inactivo hasta que lo active el administrador
                    FechaCreacion = DateTime.Now
                };

                _context.Inmuebles.Add(inmueble);
                await _context.SaveChangesAsync();

                // Procesar imagen si se envió
                if (!string.IsNullOrEmpty(request.ImagenBase64) && !string.IsNullOrEmpty(request.ImagenNombre))
                {
                    await GuardarImagenInmueble(inmueble.Id, request.ImagenBase64, request.ImagenNombre, true);
                }

                // Recargar con relaciones
                await _context.Entry(inmueble).Collection(i => i.Imagenes!).LoadAsync();

                var inmuebleDto = MapearInmuebleADto(inmueble);

                _logger.LogInformation($"Inmueble creado ID: {inmueble.Id} por propietario ID: {propietarioId}");
                return CreatedAtAction(nameof(ObtenerInmueble), new { id = inmueble.Id }, 
                    ApiResponse<InmuebleDto>.SuccessResponse(inmuebleDto, "Inmueble creado exitosamente (estado: inactivo)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear inmueble");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Habilitar/Deshabilitar un inmueble
        /// </summary>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(ApiResponse<InmuebleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<InmuebleDto>>> ActualizarEstado(int id, [FromBody] ActualizarEstadoInmuebleDto request)
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

                var inmueble = await _context.Inmuebles
                    .Include(i => i.TipoInmueble)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (inmueble == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Inmueble no encontrado"));
                }

                // Verificar que el inmueble pertenece al propietario autenticado
                if (inmueble.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, ApiResponse.ErrorResponse("No tiene permiso para modificar este inmueble"));
                }

                // Cargar colecciones por separado
                await _context.Entry(inmueble).Collection(i => i.Imagenes!).LoadAsync();
                await _context.Entry(inmueble).Collection(i => i.Contratos!).LoadAsync();

                // Log del request recibido para debugging
                _logger.LogInformation($"=== REQUEST RECIBIDO ===");
                _logger.LogInformation($"Estado string: '{request.Estado}', Activo boolean: {request.Activo}");
                
                // Obtener el estado solicitado (acepta tanto string como boolean)
                var nuevoEstadoEnum = request.ObtenerEstado();
                
                _logger.LogInformation($"Estado interpretado: {nuevoEstadoEnum}");

                // Verificar si intenta inactivar y hay contratos activos/reservados
                if (nuevoEstadoEnum == EstadoInmueble.Inactivo)
                {
                    var contratoActivo = inmueble.Contratos?
                        .FirstOrDefault(c => c.Estado == EstadoContrato.Activo || c.Estado == EstadoContrato.Reservado);
                    
                    if (contratoActivo != null)
                    {
                        string motivo = contratoActivo.Estado == EstadoContrato.Reservado
                            ? "El inmueble está Reservado con un contrato vigente"
                            : "El inmueble está No Disponible con un contrato activo";
                        
                        return BadRequest(ApiResponse.ErrorResponse($"No se puede inactivar el inmueble. {motivo}."));
                    }
                }

                // Actualizar estado
                var estadoAnterior = inmueble.Estado;
                inmueble.Estado = nuevoEstadoEnum;
                
                _logger.LogInformation($"Actualizando inmueble ID: {id} - Estado anterior: {estadoAnterior}, Nuevo estado: {inmueble.Estado}");

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"SaveChanges completado para inmueble ID: {id}");

                var inmuebleDto = MapearInmuebleADto(inmueble);

                string nuevoEstadoStr = inmueble.Estado.ToString();
                _logger.LogInformation($"Estado de inmueble ID: {id} actualizado a {nuevoEstadoStr} por propietario ID: {propietarioId}");
                return Ok(ApiResponse<InmuebleDto>.SuccessResponse(inmuebleDto, $"Estado del inmueble actualizado a {nuevoEstadoStr}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar estado de inmueble ID: {id}");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        #region Métodos Auxiliares

        private InmuebleDto MapearInmuebleADto(Inmueble inmueble)
        {
            // Obtener nombre del tipo desde la entidad relacionada
            string tipoNombre = inmueble.TipoInmueble?.Nombre ?? "Sin tipo";

            // Calcular disponibilidad basada en contratos
            string disponibilidad = "Disponible"; // Por defecto
            var contratoActivo = inmueble.Contratos?
                .FirstOrDefault(c => c.Estado == EstadoContrato.Activo || c.Estado == EstadoContrato.Reservado);
            
            if (contratoActivo != null)
            {
                disponibilidad = contratoActivo.Estado == EstadoContrato.Reservado 
                    ? "Reservado" 
                    : "No Disponible";
            }

            // Obtener URL base del servidor
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return new InmuebleDto
            {
                Id = inmueble.Id,
                Direccion = inmueble.Direccion,
                Localidad = inmueble.Localidad,
                Provincia = inmueble.Provincia,
                TipoId = inmueble.TipoId,
                TipoNombre = tipoNombre,
                Ambientes = inmueble.Ambientes,
                Superficie = inmueble.Superficie,
                Latitud = inmueble.Latitud,
                Longitud = inmueble.Longitud,
                Estado = inmueble.Estado.ToString(),
                Disponibilidad = disponibilidad,
                Precio = inmueble.Precio,
                Uso = inmueble.Uso.ToString(),
                FechaCreacion = inmueble.FechaCreacion,
                ImagenPortadaUrl = !string.IsNullOrEmpty(inmueble.ImagenPortadaUrl) 
                    ? $"{baseUrl}{inmueble.ImagenPortadaUrl}" 
                    : null,
                Imagenes = inmueble.Imagenes?.Select(img => new InmuebleImagenDto
                {
                    Id = img.Id,
                    NombreArchivo = img.NombreArchivo,
                    RutaCompleta = $"{baseUrl}{img.RutaCompleta}",
                    EsPortada = img.EsPortada,
                    Descripcion = img.Descripcion
                }).ToList() ?? new List<InmuebleImagenDto>()
            };
        }

        private async Task GuardarImagenInmueble(int inmuebleId, string imagenBase64, string nombreArchivo, bool esPortada)
        {
            try
            {
                // Decodificar base64
                var imageBytes = Convert.FromBase64String(imagenBase64);

                // Crear directorio
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "inmuebles", inmuebleId.ToString());
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Guardar archivo
                var extension = Path.GetExtension(nombreArchivo);
                var nuevoNombre = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(uploadsPath, nuevoNombre);

                await System.IO.File.WriteAllBytesAsync(rutaCompleta, imageBytes);

                // Guardar en base de datos
                var imagen = new InmuebleImagen
                {
                    InmuebleId = inmuebleId,
                    NombreArchivo = nuevoNombre,
                    RutaArchivo = $"/uploads/inmuebles/{inmuebleId}/{nuevoNombre}",
                    EsPortada = esPortada,
                    TamanoBytes = imageBytes.Length,
                    TipoMime = $"image/{extension.TrimStart('.').ToLower()}",
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };

                _context.InmuebleImagenes.Add(imagen);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al guardar imagen para inmueble ID: {inmuebleId}");
            }
        }

        #endregion
    }
}
