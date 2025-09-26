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

    public HomeController(ILogger<HomeController> logger, InmuebleRepository inmuebleRepository)
    {
        _logger = logger;
        _inmuebleRepository = inmuebleRepository;
    }

    public async Task<IActionResult> Index(string? provincia = null, string? localidad = null, 
        DateTime? fechaDesde = null, DateTime? fechaHasta = null, decimal? precioMin = null, decimal? precioMax = null)
    {
        try
        {
            // Obtener inmuebles disponibles (activos y disponibles)
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            
            // Filtrar por estado activo y disponible
            var inmueblesFiltrados = inmuebles.Where(i => 
                i.Estado == EstadoInmueble.Activo && 
                i.Disponible == true).AsQueryable();

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

            // TODO: Implementar filtro por disponibilidad de fechas cuando esté disponible el servicio de contratos

            var resultado = inmueblesFiltrados.ToList();

            // Pasar datos para los filtros
            ViewBag.ProvinciaSeleccionada = provincia;
            ViewBag.LocalidadSeleccionada = localidad;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;

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
}
