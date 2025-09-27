using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGarciaJesus.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly InmuebleRepository _inmuebleRepository;
    private readonly InmuebleImagenRepository _inmuebleImagenRepository;
    private readonly ContratoRepository _contratoRepository;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, InmuebleRepository inmuebleRepository, 
        InmuebleImagenRepository inmuebleImagenRepository, ContratoRepository contratoRepository, IConfiguration configuration)
    {
        _logger = logger;
        _inmuebleRepository = inmuebleRepository;
        _inmuebleImagenRepository = inmuebleImagenRepository;
        _contratoRepository = contratoRepository;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(string? provincia = null, string? localidad = null, 
        DateTime? fechaDesde = null, DateTime? fechaHasta = null, decimal? precioMin = null, decimal? precioMax = null,
        string? tipo = null, string? uso = null)
    {
        try
        {
            // Obtener inmuebles activos
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            var inmueblesFiltrados = inmuebles.Where(i => i.Estado == EstadoInmueble.Activo).AsQueryable();

            // Obtener todos los contratos para verificar disponibilidad
            var contratos = await _contratoRepository.GetAllAsync();
            var fechaActual = DateTime.Now.Date;

            // Si se especifican fechas, usar la fecha desde como referencia
            var fechaReferencia = fechaDesde ?? fechaActual;

            // Aplicar filtros
            if (!string.IsNullOrEmpty(provincia) && provincia != "Todas")
            {
                inmueblesFiltrados = inmueblesFiltrados.Where(i => 
                    string.IsNullOrEmpty(i.Provincia) || i.Provincia.Contains(provincia, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(localidad) && localidad != "Todas")
            {
                inmueblesFiltrados = inmueblesFiltrados.Where(i => 
                    string.IsNullOrEmpty(i.Localidad) || i.Localidad.Contains(localidad, StringComparison.OrdinalIgnoreCase));
            }

            if (precioMin.HasValue)
            {
                inmueblesFiltrados = inmueblesFiltrados.Where(i => i.Precio >= precioMin.Value);
            }

            if (precioMax.HasValue)
            {
                inmueblesFiltrados = inmueblesFiltrados.Where(i => i.Precio <= precioMax.Value);
            }

            // Filtrar por tipo de inmueble
            if (!string.IsNullOrEmpty(tipo))
            {
                if (Enum.TryParse<TipoInmueble>(tipo, out var tipoEnum))
                {
                    inmueblesFiltrados = inmueblesFiltrados.Where(i => i.Tipo == tipoEnum);
                }
            }

            // Filtrar por uso del inmueble
            if (!string.IsNullOrEmpty(uso))
            {
                if (Enum.TryParse<UsoInmueble>(uso, out var usoEnum))
                {
                    inmueblesFiltrados = inmueblesFiltrados.Where(i => i.Uso == usoEnum);
                }
            }

            // Filtrar por disponibilidad basada en contratos y fechas
            var resultado = inmueblesFiltrados.ToList().Where(inmueble => 
            {
                var disponibilidad = DeterminarDisponibilidad(inmueble, contratos, fechaReferencia, fechaHasta);
                return disponibilidad == "Disponible";
            }).ToList();

            // Pasar datos para los filtros
            ViewBag.ProvinciaSeleccionada = provincia;
            ViewBag.LocalidadSeleccionada = localidad;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.TipoSeleccionado = tipo;
            ViewBag.UsoSeleccionado = uso;

            // Obtener rangos de precios para el slider basado en TODOS los inmuebles (no filtrados)
            // Esto mantiene el rango completo disponible para el slider
            var todosConPrecio = inmuebles.Where(i => i.Precio.HasValue).ToList();
            if (todosConPrecio.Any())
            {
                ViewBag.PrecioMinimo = todosConPrecio.Min(i => i.Precio!.Value);
                ViewBag.PrecioMaximo = todosConPrecio.Max(i => i.Precio!.Value);
            }
            else
            {
                ViewBag.PrecioMinimo = 0;
                ViewBag.PrecioMaximo = 1000000; // Valor por defecto
            }

            return View(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar inmuebles públicos");
            return View(new List<Inmueble>());
        }
    }

    [Authorize(Roles = "Administrador,Empleado")]
    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Endpoint público para obtener detalles del inmueble en modal
    [HttpGet]
    public async Task<IActionResult> GetInmuebleDetails(int id)
    {
        try
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            
            if (inmueble == null || inmueble.Estado != EstadoInmueble.Activo)
            {
                return NotFound();
            }

            // Obtener Google Maps API Key
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
            
            return PartialView("_InmuebleDetailsModal", inmueble);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles del inmueble {InmuebleId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    // Endpoint público para obtener imágenes del inmueble
    [HttpGet]
    public async Task<IActionResult> GetInmuebleImagenes(int id)
    {
        try
        {
            var inmueble = await _inmuebleRepository.GetByIdAsync(id);
            
            if (inmueble == null || inmueble.Estado != EstadoInmueble.Activo)
            {
                return NotFound();
            }

            var imagenes = await _inmuebleImagenRepository.GetByInmuebleIdAsync(id);
            
            return PartialView("_InmuebleImagenesModal", imagenes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imágenes del inmueble {InmuebleId}", id);
            return PartialView("_InmuebleImagenesModal", new List<InmuebleImagen>());
        }
    }

    private string DeterminarDisponibilidad(Inmueble inmueble, IEnumerable<Contrato> contratos, DateTime fechaDesde, DateTime? fechaHasta = null)
    {
        // Si el inmueble está marcado como no disponible en la base
        if (!inmueble.Disponible)
        {
            return "NoDisponible";
        }

        // Si no se especifica fecha hasta, usar solo fecha desde
        var fechaFin = fechaHasta ?? fechaDesde;

        // Verificar si hay conflictos con contratos existentes en el rango de fechas solicitado
        var contratosConflicto = contratos.Where(c => 
            c.InmuebleId == inmueble.Id && 
            c.Estado == EstadoContrato.Activo &&
            // Verificar solapamiento de fechas
            !(fechaFin < c.FechaInicio || fechaDesde > c.FechaFin)
        );

        if (contratosConflicto.Any())
        {
            return "NoDisponible";
        }

        // Verificar si tiene contratos reservados que afecten el período
        var contratosReservados = contratos.Where(c => 
            c.InmuebleId == inmueble.Id && 
            c.Estado == EstadoContrato.Reservado &&
            // Verificar solapamiento de fechas
            !(fechaFin < c.FechaInicio || fechaDesde > c.FechaFin)
        );

        if (contratosReservados.Any())
        {
            return "Reservado";
        }

        return "Disponible";
    }
}
