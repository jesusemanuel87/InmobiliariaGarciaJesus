using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using System.Collections.Generic;
using InmobiliariaGarciaJesus.Attributes;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario)]
    public class InmueblesController : Controller
    {
        private readonly IRepository<Inmueble> _inmuebleRepository;
        private readonly IRepository<Propietario> _propietarioRepository;
        private readonly IInmuebleImagenService _imagenService;
        private readonly InmuebleImagenRepository _imagenRepository;
        private readonly IConfiguration _configuration;

        public InmueblesController(
            IRepository<Inmueble> inmuebleRepository, 
            IRepository<Propietario> propietarioRepository,
            IInmuebleImagenService imagenService,
            InmuebleImagenRepository imagenRepository,
            IConfiguration configuration)
        {
            _inmuebleRepository = inmuebleRepository;
            _propietarioRepository = propietarioRepository;
            _imagenService = imagenService;
            _imagenRepository = imagenRepository;
            _configuration = configuration;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index(string? estado = null, string? tipo = null, string? uso = null, 
            decimal? precioMin = null, decimal? precioMax = null, string? provincia = null, string? localidad = null,
            string[]? disponibilidad = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            try
            {
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                var inmueblesQuery = inmuebles.AsQueryable();

                // Restricción por rol: Los inquilinos solo ven inmuebles activos
                var userRole = HttpContext.Session.GetString("UserRole");
                
                // Establecer valores por defecto si no se han especificado filtros
                bool isFirstLoad = !Request.Query.Any();
                
                if (isFirstLoad)
                {
                    // Valores por defecto
                    provincia = provincia ?? "San Luis";
                    localidad = localidad ?? "San Luis";
                    disponibilidad = disponibilidad ?? new[] { "Disponible", "Reservado" };
                    
                    if (userRole == "Administrador" || userRole == "Empleado")
                    {
                        estado = estado ?? "Activo";
                    }
                }

                if (userRole == "Inquilino")
                {
                    inmueblesQuery = inmueblesQuery.Where(i => i.Estado == EstadoInmueble.Activo);
                    estado = "Activo"; // Forzar filtro para inquilinos
                }
                else
                {
                    // Para empleados y administradores, aplicar filtro de estado si se especifica
                    if (!string.IsNullOrEmpty(estado) && estado != "Todos")
                    {
                        if (Enum.TryParse<EstadoInmueble>(estado, out var estadoEnum))
                        {
                            inmueblesQuery = inmueblesQuery.Where(i => i.Estado == estadoEnum);
                        }
                    }
                }

                // Aplicar otros filtros
                if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
                {
                    if (Enum.TryParse<TipoInmueble>(tipo, out var tipoEnum))
                    {
                        inmueblesQuery = inmueblesQuery.Where(i => i.Tipo == tipoEnum);
                    }
                }

                if (!string.IsNullOrEmpty(uso) && uso != "Todos")
                {
                    if (Enum.TryParse<UsoInmueble>(uso, out var usoEnum))
                    {
                        inmueblesQuery = inmueblesQuery.Where(i => i.Uso == usoEnum);
                    }
                }

                if (precioMin.HasValue)
                {
                    inmueblesQuery = inmueblesQuery.Where(i => i.Precio >= precioMin.Value);
                }

                if (precioMax.HasValue)
                {
                    inmueblesQuery = inmueblesQuery.Where(i => i.Precio <= precioMax.Value);
                }

                if (!string.IsNullOrEmpty(provincia) && provincia != "Todas")
                {
                    inmueblesQuery = inmueblesQuery.Where(i => 
                        !string.IsNullOrEmpty(i.Provincia) && i.Provincia.Contains(provincia, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(localidad) && localidad != "Todas")
                {
                    inmueblesQuery = inmueblesQuery.Where(i => 
                        !string.IsNullOrEmpty(i.Localidad) && i.Localidad.Contains(localidad, StringComparison.OrdinalIgnoreCase));
                }

                // Filtro por disponibilidad (multi-select)
                if (disponibilidad != null && disponibilidad.Any())
                {
                    // Obtener contratos para determinar el estado real de disponibilidad
                    var contratoRepository = HttpContext.RequestServices.GetRequiredService<IRepository<Contrato>>();
                    var contratos = await contratoRepository.GetAllAsync();
                    var fechaActual = DateTime.Now;

                    var inmueblesConEstado = inmueblesQuery.Select(i => new
                    {
                        Inmueble = i,
                        EstadoDisponibilidad = DeterminarDisponibilidad(i, contratos, fechaActual)
                    }).ToList();

                    // Filtrar por los estados seleccionados
                    var inmueblesConDisponibilidad = inmueblesConEstado
                        .Where(x => disponibilidad.Contains(x.EstadoDisponibilidad))
                        .Select(x => x.Inmueble);

                    inmueblesQuery = inmueblesConDisponibilidad.AsQueryable();
                }

                // Filtro por fechas de disponibilidad para alquiler
                if (fechaDesde.HasValue || fechaHasta.HasValue)
                {
                    // Necesitamos verificar que el inmueble esté disponible en el rango de fechas
                    // Esto requiere verificar que no haya contratos activos que se solapen con el rango
                    var contratoRepository = HttpContext.RequestServices.GetRequiredService<IRepository<Contrato>>();
                    var contratos = await contratoRepository.GetAllAsync();

                    if (fechaDesde.HasValue && fechaHasta.HasValue)
                    {
                        // Filtrar inmuebles que NO tengan contratos activos en el rango de fechas
                        var inmueblesConContratosEnRango = contratos
                            .Where(c => c.Estado == EstadoContrato.Activo &&
                                       c.FechaInicio <= fechaHasta.Value &&
                                       c.FechaFin >= fechaDesde.Value)
                            .Select(c => c.InmuebleId)
                            .Distinct()
                            .ToList();

                        inmueblesQuery = inmueblesQuery.Where(i => !inmueblesConContratosEnRango.Contains(i.Id));
                    }
                }

                var inmueblesFiltrados = inmueblesQuery.ToList();
                
                // Cargar imágenes de portada y determinar disponibilidad para cada inmueble
                var contratoRepositoryFinal = HttpContext.RequestServices.GetRequiredService<IRepository<Contrato>>();
                var todosLosContratos = await contratoRepositoryFinal.GetAllAsync();
                var fechaActualFinal = DateTime.Now;
                var estadosDisponibilidad = new Dictionary<int, string>();

                foreach (var inmueble in inmueblesFiltrados)
                {
                    var imagenes = await _imagenRepository.GetByInmuebleIdAsync(inmueble.Id);
                    inmueble.Imagenes = imagenes.ToList();
                    
                    // Determinar y guardar el estado de disponibilidad
                    estadosDisponibilidad[inmueble.Id] = DeterminarDisponibilidad(inmueble, todosLosContratos, fechaActualFinal);
                }

                // Pasar estados de disponibilidad a la vista
                ViewBag.EstadosDisponibilidad = estadosDisponibilidad;

                // Pasar datos para los filtros
                ViewBag.EstadoSeleccionado = estado;
                ViewBag.TipoSeleccionado = tipo;
                ViewBag.UsoSeleccionado = uso;
                ViewBag.PrecioMin = precioMin;
                ViewBag.PrecioMax = precioMax;
                ViewBag.ProvinciaSeleccionada = provincia;
                ViewBag.LocalidadSeleccionada = localidad;
                ViewBag.DisponibilidadSeleccionada = disponibilidad;
                ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
                ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

                // Calcular rangos de precios
                var todosConPrecio = inmuebles.Where(i => i.Precio.HasValue).ToList();
                if (todosConPrecio.Any())
                {
                    ViewBag.PrecioMinimo = todosConPrecio.Min(i => i.Precio!.Value);
                    ViewBag.PrecioMaximo = todosConPrecio.Max(i => i.Precio!.Value);
                }
                else
                {
                    ViewBag.PrecioMinimo = 0;
                    ViewBag.PrecioMaximo = 1000000;
                }

                // Información del rol para la vista
                ViewBag.UserRole = userRole;
                ViewBag.CanViewAllStates = userRole == "Empleado" || userRole == "Administrador";
                
                // Debug: Log del rol para verificar
                Console.WriteLine($"DEBUG - UserRole: '{userRole}', CanViewAllStates: {ViewBag.CanViewAllStates}");
                
                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
                return View(inmueblesFiltrados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los inmuebles: " + ex.Message;
                return View(new List<Inmueble>());
            }
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                // Validar acceso: Solo Admin/Empleado o el propietario del inmueble
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");

                if (userRole == "Propietario")
                {
                    // Verificar que el propietario sea el dueño del inmueble
                    var usuarioRepo = HttpContext.RequestServices.GetService<IRepository<Usuario>>();
                    if (usuarioRepo != null && !string.IsNullOrEmpty(userId))
                    {
                        var usuario = (await usuarioRepo.GetAllAsync()).FirstOrDefault(u => u.Id.ToString() == userId);
                        if (usuario?.PropietarioId != inmueble.PropietarioId)
                        {
                            return RedirectToAction("AccessDenied", "Auth");
                        }
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }
                else if (userRole != "Administrador" && userRole != "Empleado")
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Inmuebles/Create
        [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> Create()
        {
            try
            {
                var propietarios = await _propietarioRepository.GetAllAsync();
                ViewBag.PropietarioId = propietarios.ToList();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la página de creación: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,Tipo,Precio,Uso")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    inmueble.FechaCreacion = DateTime.Now;
                    inmueble.Estado = EstadoInmueble.Activo;
                    
                    await _inmuebleRepository.CreateAsync(inmueble);
                    TempData["Success"] = "Inmueble creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al crear el inmueble: " + ex.Message;
                }
            }

            var propietarios = await _propietarioRepository.GetAllAsync(p => p.Estado);
            ViewBag.Propietarios = propietarios;
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                // Validar acceso: Solo Admin/Empleado o el propietario del inmueble
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");

                if (userRole == "Propietario")
                {
                    // Verificar que el propietario sea el dueño del inmueble
                    var usuarioRepo = HttpContext.RequestServices.GetService<IRepository<Usuario>>();
                    if (usuarioRepo != null && !string.IsNullOrEmpty(userId))
                    {
                        var usuario = (await usuarioRepo.GetAllAsync()).FirstOrDefault(u => u.Id.ToString() == userId);
                        if (usuario?.PropietarioId != inmueble.PropietarioId)
                        {
                            return RedirectToAction("AccessDenied", "Auth");
                        }
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }
                else if (userRole != "Administrador" && userRole != "Empleado")
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }

                var propietarios = await _propietarioRepository.GetAllAsync(p => p.Estado);
                ViewBag.Propietarios = propietarios;
                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,Tipo,Precio,Uso,FechaCreacion,Estado")] Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            // Validar acceso antes de actualizar
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            if (userRole == "Propietario")
            {
                // Verificar que el propietario sea el dueño del inmueble
                var usuarioRepo = HttpContext.RequestServices.GetService<IRepository<Usuario>>();
                if (usuarioRepo != null && !string.IsNullOrEmpty(userId))
                {
                    var usuario = (await usuarioRepo.GetAllAsync()).FirstOrDefault(u => u.Id.ToString() == userId);
                    if (usuario?.PropietarioId != inmueble.PropietarioId)
                    {
                        return RedirectToAction("AccessDenied", "Auth");
                    }
                }
                else
                {
                    return RedirectToAction("AccessDenied", "Auth");
                }
            }
            else if (userRole != "Administrador" && userRole != "Empleado")
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _inmuebleRepository.UpdateAsync(inmueble);
                    TempData["Success"] = "Inmueble actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al actualizar el inmueble: " + ex.Message;
                }
            }

            var propietarios = await _propietarioRepository.GetAllAsync(p => p.Estado);
            ViewBag.Propietarios = propietarios;
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _inmuebleRepository.DeleteAsync(id);
                TempData["Success"] = "Inmueble eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoint para obtener inmuebles con precios
        [HttpGet]
        public async Task<IActionResult> GetInmueblesConPrecios()
        {
            try
            {
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                var inmueblesConPrecios = inmuebles
                    .Where(i => i.Precio.HasValue && i.Estado == EstadoInmueble.Activo)
                    .Select(i => new { 
                        id = i.Id, 
                        direccion = i.Direccion,
                        precio = i.Precio ?? 0 
                    })
                    .ToList();
                
                return Json(inmueblesConPrecios);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: Inmuebles/UploadImages/5
        [HttpPost]
        public async Task<IActionResult> UploadImages(int id, List<IFormFile> imagenes, List<string> descripciones)
        {
            try
            {
                if (imagenes == null || !imagenes.Any())
                {
                    TempData["Error"] = "Debe seleccionar al menos una imagen";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                var inmueble = await _inmuebleRepository.GetByIdAsync(id);
                if (inmueble == null)
                {
                    TempData["Error"] = "Inmueble no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                int imagenesSubidas = 0;
                for (int i = 0; i < imagenes.Count; i++)
                {
                    if (imagenes[i] != null && imagenes[i].Length > 0)
                    {
                        var descripcion = descripciones != null && i < descripciones.Count ? descripciones[i] : null;
                        await _imagenService.GuardarImagenAsync(id, imagenes[i], descripcion);
                        imagenesSubidas++;
                    }
                }

                TempData["Success"] = $"Se subieron {imagenesSubidas} imagen(es) exitosamente";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al subir imágenes: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // POST: Inmuebles/DeleteImage/5
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id, int inmuebleId)
        {
            try
            {
                var eliminado = await _imagenService.EliminarImagenAsync(id);
                if (eliminado)
                {
                    return Json(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar la imagen" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar imagen: {ex.Message}" });
            }
        }

        // POST: Inmuebles/SetPortada/5
        [HttpPost]
        public async Task<IActionResult> SetPortada(int id, int inmuebleId)
        {
            try
            {
                var actualizado = await _imagenService.EstablecerPortadaAsync(id, inmuebleId);
                if (actualizado)
                {
                    return Json(new { success = true, message = "Imagen de portada actualizada" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar la imagen de portada" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar portada: {ex.Message}" });
            }
        }

        // GET: Inmuebles/GetImagenes/5
        [HttpGet]
        public async Task<IActionResult> GetImagenes(int id, bool readOnly = false)
        {
            try
            {
                var imagenes = await _imagenRepository.GetByInmuebleIdAsync(id);
                var viewName = readOnly ? "_ImagenesGaleriaReadOnly" : "_ImagenesGaleria";
                return PartialView(viewName, imagenes);
            }
            catch (Exception)
            {
                var viewName = readOnly ? "_ImagenesGaleriaReadOnly" : "_ImagenesGaleria";
                return PartialView(viewName, new List<InmuebleImagen>());
            }
        }

        // Método privado para determinar el estado de disponibilidad de un inmueble
        private string DeterminarDisponibilidad(Inmueble inmueble, IEnumerable<Contrato> contratos, DateTime fechaActual)
        {
            // Debug: Log para inmueble ID 1
            if (inmueble.Id == 1)
            {
                Console.WriteLine($"DEBUG - Inmueble ID 1: Disponible={inmueble.Disponible}, FechaActual={fechaActual:yyyy-MM-dd}");
                var contratosInmueble = contratos.Where(c => c.InmuebleId == inmueble.Id).ToList();
                Console.WriteLine($"DEBUG - Contratos encontrados para ID 1: {contratosInmueble.Count}");
                foreach (var c in contratosInmueble)
                {
                    Console.WriteLine($"DEBUG - Contrato: Estado={c.Estado}, Inicio={c.FechaInicio:yyyy-MM-dd}, Fin={c.FechaFin:yyyy-MM-dd}");
                }
            }

            // Si el inmueble está marcado como no disponible en la base
            if (!inmueble.Disponible)
            {
                return "NoDisponible";
            }

            // Verificar si tiene contratos activos (en curso ahora)
            var contratoActivo = contratos.FirstOrDefault(c => 
                c.InmuebleId == inmueble.Id && 
                c.Estado == EstadoContrato.Activo &&
                c.FechaInicio <= fechaActual &&
                c.FechaFin >= fechaActual);

            if (contratoActivo != null)
            {
                if (inmueble.Id == 1) Console.WriteLine($"DEBUG - ID 1: Contrato ACTIVO encontrado");
                return "NoDisponible";
            }

            // Verificar si tiene contratos futuros (reservado) - incluir tanto Activo como Reservado
            var contratoFuturo = contratos.FirstOrDefault(c => 
                c.InmuebleId == inmueble.Id && 
                (c.Estado == EstadoContrato.Activo || c.Estado == EstadoContrato.Reservado) &&
                c.FechaInicio > fechaActual);

            if (contratoFuturo != null)
            {
                if (inmueble.Id == 1) Console.WriteLine($"DEBUG - ID 1: Contrato FUTURO encontrado - RESERVADO");
                return "Reservado";
            }

            // Verificar si tiene contratos con estado "Reservado" independientemente de la fecha
            var contratoReservado = contratos.FirstOrDefault(c => 
                c.InmuebleId == inmueble.Id && 
                c.Estado == EstadoContrato.Reservado);

            if (contratoReservado != null)
            {
                if (inmueble.Id == 1) Console.WriteLine($"DEBUG - ID 1: Contrato con estado RESERVADO encontrado");
                return "Reservado";
            }

            // Si no tiene contratos activos ni futuros, está disponible
            if (inmueble.Id == 1) Console.WriteLine($"DEBUG - ID 1: Sin contratos - DISPONIBLE");
            return "Disponible";
        }
    }
}
