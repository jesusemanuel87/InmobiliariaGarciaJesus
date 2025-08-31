using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContratoApiController : ControllerBase
    {
        private readonly IContratoService _contratoService;

        public ContratoApiController(IContratoService contratoService)
        {
            _contratoService = contratoService;
        }

        // GET: api/ContratoApi/unavailable-dates/{inmuebleId}
        [HttpGet("unavailable-dates/{inmuebleId}")]
        public async Task<IActionResult> GetUnavailableDates(int inmuebleId)
        {
            try
            {
                var unavailableDates = await _contratoService.GetUnavailableDatesAsync(inmuebleId);
                
                var result = unavailableDates.Select(d => new
                {
                    start = d.FechaInicio.ToString("yyyy-MM-dd"),
                    end = d.FechaFin.ToString("yyyy-MM-dd")
                });
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET: api/ContratoApi/next-available-date/{inmuebleId}
        [HttpGet("next-available-date/{inmuebleId}")]
        public async Task<IActionResult> GetNextAvailableDate(int inmuebleId)
        {
            try
            {
                var nextDate = await _contratoService.GetNextAvailableDateAsync(inmuebleId);
                
                return Ok(new { date = nextDate?.ToString("yyyy-MM-dd") });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
