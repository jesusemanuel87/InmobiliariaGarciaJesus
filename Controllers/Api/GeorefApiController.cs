using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Models.DTOs;

namespace InmobiliariaGarciaJesus.Controllers.Api
{
    /// <summary>
    /// API para obtener provincias y localidades con fallback automático
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GeorefApiController : ControllerBase
    {
        private readonly GeorefService _georefService;
        private readonly ILogger<GeorefApiController> _logger;

        public GeorefApiController(
            GeorefService georefService,
            ILogger<GeorefApiController> logger)
        {
            _georefService = georefService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene lista de provincias (primero desde BD local, fallback a API Georef)
        /// </summary>
        /// <returns>Lista de provincias ordenadas alfabéticamente</returns>
        [HttpGet("provincias")]
        [ProducesResponseType(typeof(List<GeorefService.ProvinciaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GeorefService.ProvinciaDto>>> ObtenerProvincias()
        {
            try
            {
                var provincias = await _georefService.ObtenerProvinciasAsync();
                return Ok(provincias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener provincias");
                return StatusCode(500, new { error = "Error al obtener provincias" });
            }
        }

        /// <summary>
        /// Obtiene lista de localidades de una provincia (primero desde BD local, fallback a API Georef)
        /// </summary>
        /// <param name="provinciaNombre">Nombre de la provincia</param>
        /// <returns>Lista de localidades ordenadas alfabéticamente</returns>
        [HttpGet("localidades/{provinciaNombre}")]
        [ProducesResponseType(typeof(List<GeorefService.LocalidadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GeorefService.LocalidadDto>>> ObtenerLocalidades(string provinciaNombre)
        {
            try
            {
                var localidades = await _georefService.ObtenerLocalidadesAsync(provinciaNombre);
                return Ok(localidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener localidades de {provinciaNombre}");
                return StatusCode(500, new { error = "Error al obtener localidades" });
            }
        }

        /// <summary>
        /// Fuerza la sincronización de provincias desde API Georef (solo admin)
        /// </summary>
        [HttpPost("sync/provincias")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SincronizarProvincias()
        {
            try
            {
                var exitoso = await _georefService.SincronizarProvinciasAsync(force: true);
                
                if (exitoso)
                {
                    return Ok(new { message = "Provincias sincronizadas correctamente" });
                }

                return BadRequest(new { error = "No se pudieron sincronizar las provincias" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar provincias");
                return StatusCode(500, new { error = "Error al sincronizar provincias" });
            }
        }

        /// <summary>
        /// Fuerza la sincronización de localidades de una provincia desde API Georef (solo admin)
        /// </summary>
        [HttpPost("sync/localidades/{provinciaNombre}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SincronizarLocalidades(string provinciaNombre)
        {
            try
            {
                var exitoso = await _georefService.SincronizarLocalidadesAsync(provinciaNombre, force: true);
                
                if (exitoso)
                {
                    return Ok(new { message = $"Localidades de {provinciaNombre} sincronizadas correctamente" });
                }

                return BadRequest(new { error = "No se pudieron sincronizar las localidades" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al sincronizar localidades de {provinciaNombre}");
                return StatusCode(500, new { error = "Error al sincronizar localidades" });
            }
        }
    }
}
