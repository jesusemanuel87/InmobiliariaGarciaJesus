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
    /// API Controller para gestión de contratos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class ContratosApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<ContratosApiController> _logger;

        public ContratosApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<ContratosApiController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Listar contratos de un inmueble específico con sus pagos
        /// </summary>
        [HttpGet("inmueble/{inmuebleId}")]
        [ProducesResponseType(typeof(ApiResponse<List<ContratoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<List<ContratoDto>>>> ListarContratosPorInmueble(int inmuebleId)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                // Verificar que el inmueble pertenece al propietario
                var inmueble = await _context.Inmuebles
                    .FirstOrDefaultAsync(i => i.Id == inmuebleId);

                if (inmueble == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Inmueble no encontrado"));
                }

                if (inmueble.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, ApiResponse.ErrorResponse("No tiene permiso para acceder a los contratos de este inmueble"));
                }

                // Obtener contratos con sus relaciones
                var contratos = await _context.Contratos
                    .Include(c => c.Inmueble)
                        .ThenInclude(i => i!.Imagenes)
                    .Include(c => c.Inquilino)
                    .Where(c => c.InmuebleId == inmuebleId)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                var contratosDto = new List<ContratoDto>();

                foreach (var contrato in contratos)
                {
                    // Obtener pagos del contrato
                    var pagos = await _context.Pagos
                        .Where(p => p.ContratoId == contrato.Id)
                        .OrderBy(p => p.FechaVencimiento)
                        .ToListAsync();

                    contratosDto.Add(MapearContratoADto(contrato, pagos));
                }

                return Ok(ApiResponse<List<ContratoDto>>.SuccessResponse(contratosDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al listar contratos del inmueble ID: {inmuebleId}");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtener detalle de un contrato específico con sus pagos
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ContratoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ContratoDto>>> ObtenerContrato(int id)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                        .ThenInclude(i => i!.Imagenes)
                    .Include(c => c.Inquilino)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (contrato == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Contrato no encontrado"));
                }

                // Verificar que el inmueble pertenece al propietario
                if (contrato.Inmueble?.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, ApiResponse.ErrorResponse("No tiene permiso para acceder a este contrato"));
                }

                // Obtener pagos del contrato
                var pagos = await _context.Pagos
                    .Where(p => p.ContratoId == contrato.Id)
                    .OrderBy(p => p.FechaVencimiento)
                    .ToListAsync();

                var contratoDto = MapearContratoADto(contrato, pagos);

                return Ok(ApiResponse<ContratoDto>.SuccessResponse(contratoDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener contrato ID: {id}");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Listar todos los contratos de los inmuebles del propietario
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ContratoDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<ContratoDto>>>> ListarTodosLosContratos()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                // Obtener IDs de inmuebles del propietario
                var inmueblesIds = await _context.Inmuebles
                    .Where(i => i.PropietarioId == propietarioId.Value)
                    .Select(i => i.Id)
                    .ToListAsync();

                // Obtener contratos
                var contratos = await _context.Contratos
                    .Include(c => c.Inmueble)
                        .ThenInclude(i => i!.Imagenes)
                    .Include(c => c.Inquilino)
                    .Where(c => inmueblesIds.Contains(c.InmuebleId))
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                var contratosDto = new List<ContratoDto>();

                foreach (var contrato in contratos)
                {
                    // Obtener pagos del contrato
                    var pagos = await _context.Pagos
                        .Where(p => p.ContratoId == contrato.Id)
                        .OrderBy(p => p.FechaVencimiento)
                        .ToListAsync();

                    contratosDto.Add(MapearContratoADto(contrato, pagos));
                }

                return Ok(ApiResponse<List<ContratoDto>>.SuccessResponse(contratosDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar todos los contratos");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        #region Métodos Auxiliares

        private ContratoDto MapearContratoADto(Contrato contrato, List<Pago> pagos)
        {
            return new ContratoDto
            {
                Id = contrato.Id,
                FechaInicio = contrato.FechaInicio,
                FechaFin = contrato.FechaFin,
                Precio = contrato.Precio,
                Estado = contrato.Estado.ToString(),
                FechaCreacion = contrato.FechaCreacion,
                MotivoCancelacion = contrato.MotivoCancelacion,
                FechaFinalizacionReal = contrato.FechaFinalizacionReal,
                MultaFinalizacion = contrato.MultaFinalizacion,
                MesesAdeudados = contrato.MesesAdeudados,
                ImporteAdeudado = contrato.ImporteAdeudado,
                Inmueble = new InmuebleContratoDto
                {
                    Id = contrato.Inmueble?.Id ?? 0,
                    Direccion = contrato.Inmueble?.Direccion ?? string.Empty,
                    Localidad = contrato.Inmueble?.Localidad,
                    Provincia = contrato.Inmueble?.Provincia,
                    Ambientes = contrato.Inmueble?.Ambientes ?? 0,
                    ImagenPortadaUrl = contrato.Inmueble?.ImagenPortadaUrl
                },
                Inquilino = new InquilinoContratoDto
                {
                    Id = contrato.Inquilino?.Id ?? 0,
                    NombreCompleto = contrato.Inquilino?.NombreCompleto ?? string.Empty,
                    Dni = contrato.Inquilino?.Dni ?? string.Empty,
                    Telefono = contrato.Inquilino?.Telefono ?? string.Empty,
                    Email = contrato.Inquilino?.Email ?? string.Empty
                },
                Pagos = pagos.Select(p => new PagoDto
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    FechaPago = p.FechaPago,
                    ContratoId = p.ContratoId,
                    Importe = p.Importe,
                    Intereses = p.Intereses,
                    Multas = p.Multas,
                    TotalAPagar = p.TotalAPagar,
                    FechaVencimiento = p.FechaVencimiento,
                    Estado = p.Estado.ToString(),
                    MetodoPago = p.MetodoPago?.ToString(),
                    Observaciones = p.Observaciones,
                    FechaCreacion = p.FechaCreacion
                }).ToList()
            };
        }

        #endregion
    }
}
