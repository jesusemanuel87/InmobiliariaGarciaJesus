using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.Common;
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
    private readonly TipoInmuebleRepository _tipoInmuebleRepository;
    private readonly PagoRepository _pagoRepository;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, InmuebleRepository inmuebleRepository, 
        InmuebleImagenRepository inmuebleImagenRepository, ContratoRepository contratoRepository, 
        TipoInmuebleRepository tipoInmuebleRepository, PagoRepository pagoRepository, IConfiguration configuration)
    {
        _logger = logger;
        _inmuebleRepository = inmuebleRepository;
        _inmuebleImagenRepository = inmuebleImagenRepository;
        _contratoRepository = contratoRepository;
        _tipoInmuebleRepository = tipoInmuebleRepository;
        _pagoRepository = pagoRepository;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? provincia = null, 
        string? localidad = null, 
        DateTime? fechaDesde = null, 
        DateTime? fechaHasta = null, 
        decimal? precioMin = null, 
        decimal? precioMax = null,
        string? tipo = null, 
        string? uso = null)
    {
        try
        {
            // Configuración de paginación
            const int pageSize = 12; // Mostrar 12 inmuebles por página
            
            // Obtener tipos de inmueble activos para el filtro
            var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();

            // Convertir tipo de string a ID
            int? tipoId = null;
            if (!string.IsNullOrEmpty(tipo))
            {
                var tipoEncontrado = tiposActivos.FirstOrDefault(t => t.Nombre.Equals(tipo, StringComparison.OrdinalIgnoreCase));
                tipoId = tipoEncontrado?.Id;
            }

            // Convertir uso de string a enum
            UsoInmueble? usoEnum = null;
            if (!string.IsNullOrEmpty(uso) && Enum.TryParse<UsoInmueble>(uso, out var usoTemp))
            {
                usoEnum = usoTemp;
            }

            // ✅ OPTIMIZACIÓN: Solo traer los inmuebles de la página actual con filtros aplicados en SQL
            var pagedResult = await _inmuebleRepository.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                provincia: provincia,
                localidad: localidad,
                precioMin: precioMin,
                precioMax: precioMax,
                estado: EstadoInmueble.Activo,
                tipoId: tipoId,
                uso: usoEnum
            );

            // ✅ OPTIMIZACIÓN: Solo traer contratos de los inmuebles de la página actual
            var inmuebleIds = pagedResult.Items.Select(i => i.Id).ToList();
            var contratosRelevantes = await _contratoRepository.GetByInmuebleIdsAsync(inmuebleIds);
            
            var fechaActual = DateTime.Now.Date;
            var fechaReferencia = fechaDesde ?? fechaActual;

            // Filtrar por disponibilidad (solo los 12 inmuebles actuales)
            var inmueblesDisponibles = pagedResult.Items.Where(inmueble => 
            {
                var disponibilidad = DeterminarDisponibilidad(inmueble, contratosRelevantes, fechaReferencia, fechaHasta);
                return disponibilidad == "Disponible";
            }).ToList();

            // Actualizar el resultado paginado con los inmuebles disponibles
            var resultadoFinal = new PagedResult<Inmueble>(
                inmueblesDisponibles,
                pagedResult.TotalCount, // Mantener el total original para la paginación
                page,
                pageSize
            );

            // Pasar datos para los filtros
            ViewBag.ProvinciaSeleccionada = provincia;
            ViewBag.LocalidadSeleccionada = localidad;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.TipoSeleccionado = tipo;
            ViewBag.UsoSeleccionado = uso;
            ViewBag.TiposInmueble = tiposActivos;

            // Rangos de precios para el slider (calcular desde una query rápida)
            ViewBag.PrecioMinimo = 0;
            ViewBag.PrecioMaximo = 1000000; // Valor por defecto, podría optimizarse con query específico

            _logger.LogInformation("Página {Page}: Cargados {Count} inmuebles de {Total} total", 
                page, inmueblesDisponibles.Count, pagedResult.TotalCount);

            return View(resultadoFinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar inmuebles públicos");
            return View(new PagedResult<Inmueble>(new List<Inmueble>(), 0, 1, 12));
        }
    }

    [Authorize(Roles = "Administrador,Empleado")]
    public async Task<IActionResult> Dashboard()
    {
        try
        {
            // Obtener estadísticas del dashboard
            var totalInmuebles = (await _inmuebleRepository.GetAllAsync()).Count();
            var contratosActivos = (await _contratoRepository.GetAllAsync()).Count(c => c.Estado == EstadoContrato.Activo);
            
            // Obtener pagos pendientes (estado Pendiente)
            var pagosPendientes = (await _pagoRepository.GetAllAsync()).Count(p => p.Estado == EstadoPago.Pendiente);
            
            // Calcular ingresos del mes actual
            var mesActual = DateTime.Now.Month;
            var añoActual = DateTime.Now.Year;
            var ingresosMes = (await _pagoRepository.GetAllAsync())
                .Where(p => p.Estado == EstadoPago.Pagado && 
                           p.FechaPago.HasValue && 
                           p.FechaPago.Value.Month == mesActual && 
                           p.FechaPago.Value.Year == añoActual)
                .Sum(p => p.Importe);

            // Pasar datos a la vista
            ViewBag.TotalInmuebles = totalInmuebles;
            ViewBag.ContratosActivos = contratosActivos;
            ViewBag.PagosPendientes = pagosPendientes;
            ViewBag.IngresosMes = ingresosMes;

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar datos del dashboard");
            
            // Valores por defecto en caso de error
            ViewBag.TotalInmuebles = 0;
            ViewBag.ContratosActivos = 0;
            ViewBag.PagosPendientes = 0;
            ViewBag.IngresosMes = 0;

            return View();
        }
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
