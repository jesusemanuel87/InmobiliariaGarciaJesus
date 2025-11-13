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
    /// API Controller para gestión de pagos desde aplicaciones móviles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class PagosApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<PagosApiController> _logger;

        public PagosApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            NotificacionService notificacionService,
            ILogger<PagosApiController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener pagos pendientes de un contrato
        /// </summary>
        [HttpGet("contrato/{contratoId}/pendientes")]
        [ProducesResponseType(typeof(List<PagoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<PagoDto>>> ObtenerPagosPendientes(int contratoId)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                // Verificar que el contrato pertenece a un inmueble del propietario
                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                    .FirstOrDefaultAsync(c => c.Id == contratoId);

                if (contrato == null)
                {
                    return NotFound(new { error = "Contrato no encontrado" });
                }

                if (contrato.Inmueble?.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, new { error = "No tiene permiso para acceder a este contrato" });
                }

                // Obtener pagos pendientes
                var pagosEntities = await _context.Pagos
                    .Where(p => p.ContratoId == contratoId && p.Estado == EstadoPago.Pendiente)
                    .OrderBy(p => p.FechaVencimiento)
                    .ToListAsync();

                var pagos = pagosEntities.Select(p => new PagoDto
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    ContratoId = p.ContratoId,
                    Importe = p.Importe,
                    Intereses = p.Intereses,
                    Multas = p.Multas,
                    TotalAPagar = p.TotalAPagar,
                    FechaVencimiento = p.FechaVencimiento,
                    FechaPago = p.FechaPago,
                    Estado = p.Estado.ToString(),
                    MetodoPago = p.MetodoPago?.ToString(),
                    Observaciones = p.Observaciones,
                    FechaCreacion = p.FechaCreacion
                }).ToList();

                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener pagos pendientes del contrato {contratoId}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Registrar pago de alquiler y notificar al propietario
        /// </summary>
        [HttpPost("{pagoId}/registrar")]
        [ProducesResponseType(typeof(PagoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagoDto>> RegistrarPago(
            int pagoId,
            [FromBody] RegistrarPagoDto request)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Datos inválidos", details = errors });
                }

                // Obtener el pago
                var pago = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.Id == pagoId);

                if (pago == null)
                {
                    return NotFound(new { error = "Pago no encontrado" });
                }

                // Obtener el contrato con sus relaciones
                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                    .Include(c => c.Inquilino)
                    .FirstOrDefaultAsync(c => c.Id == pago.ContratoId);

                if (contrato == null)
                {
                    return NotFound(new { error = "Contrato no encontrado" });
                }

                // Verificar permisos
                if (contrato.Inmueble?.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, new { error = "No tiene permiso para registrar este pago" });
                }

                // Verificar que no esté ya pagado
                if (pago.Estado == EstadoPago.Pagado)
                {
                    return BadRequest(new { error = "El pago ya fue registrado" });
                }

                // Registrar el pago
                pago.FechaPago = DateTime.Now;
                pago.Estado = EstadoPago.Pagado;
                pago.MetodoPago = request.MetodoPago;
                pago.Observaciones = request.Observaciones;

                await _context.SaveChangesAsync();

                // Enviar notificación al propietario
                try
                {
                    await _notificacionService.NotificarPagoRegistrado(pago, propietarioId.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Error al enviar notificación de pago {pagoId}, pero el pago fue registrado");
                    // No fallar la operación si falla el email
                }

                var pagoDto = new PagoDto
                {
                    Id = pago.Id,
                    Numero = pago.Numero,
                    ContratoId = pago.ContratoId,
                    Importe = pago.Importe,
                    Intereses = pago.Intereses,
                    Multas = pago.Multas,
                    TotalAPagar = pago.TotalAPagar,
                    FechaVencimiento = pago.FechaVencimiento,
                    FechaPago = pago.FechaPago,
                    Estado = pago.Estado.ToString(),
                    MetodoPago = pago.MetodoPago?.ToString(),
                    Observaciones = pago.Observaciones,
                    FechaCreacion = pago.FechaCreacion
                };

                _logger.LogInformation($"Pago {pagoId} registrado exitosamente por propietario {propietarioId}");
                return Ok(pagoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al registrar pago {pagoId}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener historial de pagos de un contrato
        /// </summary>
        [HttpGet("contrato/{contratoId}/historial")]
        [ProducesResponseType(typeof(List<PagoDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PagoDto>>> ObtenerHistorialPagos(int contratoId)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(new { error = "No autorizado" });
                }

                var contrato = await _context.Contratos
                    .Include(c => c.Inmueble)
                    .FirstOrDefaultAsync(c => c.Id == contratoId);

                if (contrato == null)
                {
                    return NotFound(new { error = "Contrato no encontrado" });
                }

                if (contrato.Inmueble?.PropietarioId != propietarioId.Value)
                {
                    return StatusCode(403, new { error = "No tiene permiso" });
                }

                var pagosEntities = await _context.Pagos
                    .Where(p => p.ContratoId == contratoId)
                    .OrderByDescending(p => p.FechaVencimiento)
                    .ToListAsync();

                var pagos = pagosEntities.Select(p => new PagoDto
                {
                    Id = p.Id,
                    Numero = p.Numero,
                    ContratoId = p.ContratoId,
                    Importe = p.Importe,
                    Intereses = p.Intereses,
                    Multas = p.Multas,
                    TotalAPagar = p.TotalAPagar,
                    FechaVencimiento = p.FechaVencimiento,
                    FechaPago = p.FechaPago,
                    Estado = p.Estado.ToString(),
                    MetodoPago = p.MetodoPago?.ToString(),
                    Observaciones = p.Observaciones,
                    FechaCreacion = p.FechaCreacion
                }).ToList();

                return Ok(pagos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener historial de pagos");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }

    /// <summary>
    /// DTO para registrar un pago
    /// </summary>
    public class RegistrarPagoDto
    {
        public MetodoPago MetodoPago { get; set; }
        public string? Observaciones { get; set; }
    }
}
