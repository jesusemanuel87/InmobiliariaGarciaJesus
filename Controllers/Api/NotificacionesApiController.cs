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
    /// API Controller para gestionar notificaciones in-app
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class NotificacionesApiController : ControllerBase
    {
        private readonly InmobiliariaDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<NotificacionesApiController> _logger;

        public NotificacionesApiController(
            InmobiliariaDbContext context,
            JwtService jwtService,
            ILogger<NotificacionesApiController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las notificaciones del propietario (leídas y no leídas)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> ObtenerNotificaciones()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var notificaciones = await _context.Notificaciones
                    .Where(n => n.PropietarioId == propietarioId.Value)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(50) // Limitar a últimas 50 notificaciones
                    .Select(n => new NotificacionDto
                    {
                        Id = n.Id,
                        Tipo = n.Tipo,
                        Titulo = n.Titulo,
                        Mensaje = n.Mensaje,
                        Datos = n.Datos,
                        Leida = n.Leida,
                        FechaCreacion = n.FechaCreacion,
                        FechaLeida = n.FechaLeida
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<NotificacionDto>>.SuccessResponse(notificaciones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtener solo las notificaciones NO LEÍDAS
        /// </summary>
        [HttpGet("no-leidas")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificacionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<NotificacionDto>>>> ObtenerNotificacionesNoLeidas()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var notificaciones = await _context.Notificaciones
                    .Where(n => n.PropietarioId == propietarioId.Value && !n.Leida)
                    .OrderByDescending(n => n.FechaCreacion)
                    .Select(n => new NotificacionDto
                    {
                        Id = n.Id,
                        Tipo = n.Tipo,
                        Titulo = n.Titulo,
                        Mensaje = n.Mensaje,
                        Datos = n.Datos,
                        Leida = n.Leida,
                        FechaCreacion = n.FechaCreacion,
                        FechaLeida = n.FechaLeida
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<NotificacionDto>>.SuccessResponse(notificaciones));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones no leídas");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Obtener contador de notificaciones no leídas (para badge)
        /// </summary>
        [HttpGet("contador")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<int>>> ObtenerContadorNoLeidas()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var contador = await _context.Notificaciones
                    .Where(n => n.PropietarioId == propietarioId.Value && !n.Leida)
                    .CountAsync();

                return Ok(ApiResponse<int>.SuccessResponse(contador));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener contador de notificaciones");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Marcar una notificación como leída
        /// </summary>
        [HttpPatch("{notificacionId}/marcar-leida")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> MarcarComoLeida(int notificacionId)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var notificacion = await _context.Notificaciones
                    .FirstOrDefaultAsync(n => n.Id == notificacionId && n.PropietarioId == propietarioId.Value);

                if (notificacion == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Notificación no encontrada"));
                }

                if (!notificacion.Leida)
                {
                    notificacion.Leida = true;
                    notificacion.FechaLeida = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return Ok(ApiResponse.SuccessResponse("Notificación marcada como leída"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al marcar notificación {notificacionId} como leída");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Marcar TODAS las notificaciones como leídas
        /// </summary>
        [HttpPatch("marcar-todas-leidas")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> MarcarTodasComoLeidas()
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var notificacionesNoLeidas = await _context.Notificaciones
                    .Where(n => n.PropietarioId == propietarioId.Value && !n.Leida)
                    .ToListAsync();

                foreach (var notificacion in notificacionesNoLeidas)
                {
                    notificacion.Leida = true;
                    notificacion.FechaLeida = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResponse($"{notificacionesNoLeidas.Count} notificaciones marcadas como leídas"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }

        /// <summary>
        /// Eliminar una notificación
        /// </summary>
        [HttpDelete("{notificacionId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> EliminarNotificacion(int notificacionId)
        {
            try
            {
                var propietarioId = _jwtService.ObtenerPropietarioId(User);
                if (propietarioId == null)
                {
                    return Unauthorized(ApiResponse.ErrorResponse("No autorizado"));
                }

                var notificacion = await _context.Notificaciones
                    .FirstOrDefaultAsync(n => n.Id == notificacionId && n.PropietarioId == propietarioId.Value);

                if (notificacion == null)
                {
                    return NotFound(ApiResponse.ErrorResponse("Notificación no encontrada"));
                }

                _context.Notificaciones.Remove(notificacion);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse.SuccessResponse("Notificación eliminada"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar notificación {notificacionId}");
                return StatusCode(500, ApiResponse.ErrorResponse("Error interno del servidor"));
            }
        }
    }

    /// <summary>
    /// DTO para Notificación
    /// </summary>
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? Datos { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLeida { get; set; }
    }
}
