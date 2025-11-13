using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.DTOs;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Controllers.Api
{
    /// <summary>
    /// API pública para obtener tipos de inmuebles
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TiposInmuebleApiController : ControllerBase
    {
        private readonly TipoInmuebleRepository _tipoInmuebleRepository;
        private readonly ILogger<TiposInmuebleApiController> _logger;

        public TiposInmuebleApiController(
            TipoInmuebleRepository tipoInmuebleRepository,
            ILogger<TiposInmuebleApiController> logger)
        {
            _tipoInmuebleRepository = tipoInmuebleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los tipos de inmuebles activos
        /// </summary>
        /// <returns>Lista de tipos de inmuebles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TipoInmuebleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<TipoInmuebleDto>>> ObtenerTiposInmueble()
        {
            try
            {
                var tipos = await _tipoInmuebleRepository.GetAllAsync();
                
                // Filtrar solo los activos y mapear a DTO
                var tiposDto = tipos
                    .Where(t => t.Estado) // Solo tipos activos
                    .OrderBy(t => t.Nombre)
                    .Select(t => new TipoInmuebleDto
                    {
                        Id = t.Id,
                        Nombre = t.Nombre,
                        Descripcion = t.Descripcion
                    })
                    .ToList();

                _logger.LogInformation($"Tipos de inmuebles obtenidos para API: {tiposDto.Count}");
                
                return Ok(tiposDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de inmuebles para API");
                return StatusCode(500, new { error = "Error al obtener tipos de inmuebles" });
            }
        }

        /// <summary>
        /// Obtiene un tipo de inmueble específico por ID
        /// </summary>
        /// <param name="id">ID del tipo de inmueble</param>
        /// <returns>Tipo de inmueble</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TipoInmuebleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TipoInmuebleDto>> ObtenerTipoInmueblePorId(int id)
        {
            try
            {
                var tipo = await _tipoInmuebleRepository.GetByIdAsync(id);
                
                if (tipo == null)
                {
                    return NotFound(new { error = $"Tipo de inmueble con ID {id} no encontrado" });
                }

                var tipoDto = new TipoInmuebleDto
                {
                    Id = tipo.Id,
                    Nombre = tipo.Nombre,
                    Descripcion = tipo.Descripcion
                };

                return Ok(tipoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tipo de inmueble ID: {id}");
                return StatusCode(500, new { error = "Error al obtener tipo de inmueble" });
            }
        }
    }

    /// <summary>
    /// DTO para Tipo de Inmueble (para API)
    /// </summary>
    public class TipoInmuebleDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
