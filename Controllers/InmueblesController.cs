using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.Common;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Extensions;
using System.Collections.Generic;
using InmobiliariaGarciaJesus.Attributes;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario)]
    public class InmueblesController : Controller
    {
        private readonly InmuebleRepository _inmuebleRepository;
        private readonly IRepository<Propietario> _propietarioRepository;
        private readonly TipoInmuebleRepository _tipoInmuebleRepository;
        private readonly IInmuebleImagenService _imagenService;
        private readonly InmuebleImagenRepository _imagenRepository;
        private readonly ContratoRepository _contratoRepository;
        private readonly IConfiguration _configuration;

        public InmueblesController(
            InmuebleRepository inmuebleRepository, 
            IRepository<Propietario> propietarioRepository,
            TipoInmuebleRepository tipoInmuebleRepository,
            IInmuebleImagenService imagenService,
            InmuebleImagenRepository imagenRepository,
            ContratoRepository contratoRepository,
            IConfiguration configuration)
        {
            _inmuebleRepository = inmuebleRepository;
            _propietarioRepository = propietarioRepository;
            _tipoInmuebleRepository = tipoInmuebleRepository;
            _imagenService = imagenService;
            _imagenRepository = imagenRepository;
            _contratoRepository = contratoRepository;
            _configuration = configuration;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index(
            int page = 1,
            string? estado = null, 
            string? tipo = null, 
            string? uso = null, 
            decimal? precioMin = null, 
            decimal? precioMax = null, 
            string? provincia = null, 
            string? localidad = null,
            string[]? disponibilidad = null, 
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null)
        {
            try
            {
                // Validar sesión antes de continuar
                var sessionValidation = this.RedirectToLoginIfInvalidSession();
                if (sessionValidation != null) return sessionValidation;

                var userRole = this.GetUserRole();
                
                // Obtener tipos de inmueble activos para el filtro
                var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();
                
                // Configuración de paginación (20 items para módulo interno)
                const int pageSize = 20;
                
                // Establecer valores por defecto
                bool isFirstLoad = !Request.Query.Any();
                if (isFirstLoad && (userRole == "Administrador" || userRole == "Empleado"))
                {
                    estado = estado ?? "Activo";
                }
                
                // Determinar estado según rol
                EstadoInmueble? estadoEnum = null;
                if (userRole == "Inquilino")
                {
                    estadoEnum = EstadoInmueble.Activo;
                    estado = "Activo";
                }
                else if (!string.IsNullOrEmpty(estado) && estado != "Todos")
                {
                    Enum.TryParse<EstadoInmueble>(estado, out var tempEstado);
                    estadoEnum = tempEstado;
                }

                // Convertir tipo de string a ID
                int? tipoId = null;
                if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
                {
                    var tipoEncontrado = tiposActivos.FirstOrDefault(t => t.Nombre.Equals(tipo, StringComparison.OrdinalIgnoreCase));
                    tipoId = tipoEncontrado?.Id;
                }

                // Convertir uso de string a enum
                UsoInmueble? usoEnum = null;
                if (!string.IsNullOrEmpty(uso) && uso != "Todos" && Enum.TryParse<UsoInmueble>(uso, out var tempUso))
                {
                    usoEnum = tempUso;
                }

                // ✅ OPTIMIZACIÓN: Solo traer inmuebles de la página actual con filtros en SQL
                var pagedResult = await _inmuebleRepository.GetPagedAsync(
                    page: page,
                    pageSize: pageSize,
                    provincia: provincia != "Todas" ? provincia : null,
                    localidad: localidad != "Todas" ? localidad : null,
                    precioMin: precioMin,
                    precioMax: precioMax,
                    estado: estadoEnum,
                    tipoId: tipoId,
                    uso: usoEnum
                );

                // ✅ OPTIMIZACIÓN: Solo traer contratos de los inmuebles de la página actual
                var inmuebleIds = pagedResult.Items.Select(i => i.Id).ToList();
                var contratosRelevantes = await _contratoRepository.GetByInmuebleIdsAsync(inmuebleIds);
                
                var fechaActual = DateTime.Now;
                var estadosDisponibilidad = new Dictionary<int, string>();

                // Determinar disponibilidad solo de los 20 inmuebles actuales
                foreach (var inmueble in pagedResult.Items)
                {
                    estadosDisponibilidad[inmueble.Id] = DeterminarDisponibilidad(inmueble, contratosRelevantes, fechaActual);
                }

                // Aplicar filtro de disponibilidad si es necesario (en memoria, pero solo 20 items)
                var inmueblesFiltrados = pagedResult.Items;
                if (disponibilidad != null && disponibilidad.Any())
                {
                    inmueblesFiltrados = inmueblesFiltrados
                        .Where(i => disponibilidad.Contains(estadosDisponibilidad[i.Id]))
                        .ToList();
                }

                // Crear resultado paginado final
                var resultadoFinal = new PagedResult<Inmueble>(
                    inmueblesFiltrados,
                    pagedResult.TotalCount,
                    page,
                    pageSize
                );

                // Pasar estados de disponibilidad a la vista
                ViewBag.EstadosDisponibilidad = estadosDisponibilidad;
                ViewBag.PagedResult = resultadoFinal;

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
                ViewBag.TiposInmueble = tiposActivos;

                // Rangos de precios (valores por defecto, podrían optimizarse con query específico)
                ViewBag.PrecioMinimo = 0;
                ViewBag.PrecioMaximo = 1000000;

                // Información del rol para la vista
                ViewBag.UserRole = userRole;
                ViewBag.CanViewAllStates = userRole == "Empleado" || userRole == "Administrador";
                
                ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
                return View(resultadoFinal);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los inmuebles: " + ex.Message;
                ViewBag.UserRole = this.GetUserRole();
                return View(new PagedResult<Inmueble>(new List<Inmueble>(), 0, 1, 20));
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
                var propietarios = await _propietarioRepository.GetAllAsync(p => p.Estado);
                var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();
                
                ViewBag.Propietarios = propietarios.ToList();
                ViewBag.TiposInmueble = tiposActivos;
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
        public async Task<IActionResult> Create([Bind("Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,TipoId,Precio,Uso,Provincia,Localidad")] Inmueble inmueble)
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
            var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();
            
            ViewBag.Propietarios = propietarios;
            ViewBag.TiposInmueble = tiposActivos;
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
                var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();
                
                ViewBag.Propietarios = propietarios;
                ViewBag.TiposInmueble = tiposActivos;
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,TipoId,Precio,Uso,FechaCreacion,Estado,Provincia,Localidad")] Inmueble inmueble)
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
            var tiposActivos = await _tipoInmuebleRepository.GetActivosAsync();
            
            ViewBag.Propietarios = propietarios;
            ViewBag.TiposInmueble = tiposActivos;
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
           /*
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
            */

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
                return "NoDisponible";
            }

            // Verificar si tiene contratos futuros (reservado) - incluir tanto Activo como Reservado
            var contratoFuturo = contratos.FirstOrDefault(c => 
                c.InmuebleId == inmueble.Id && 
                (c.Estado == EstadoContrato.Activo || c.Estado == EstadoContrato.Reservado) &&
                c.FechaInicio > fechaActual);

            if (contratoFuturo != null)
            {
                return "Reservado";
            }

            // Verificar si tiene contratos con estado "Reservado" independientemente de la fecha
            var contratoReservado = contratos.FirstOrDefault(c => 
                c.InmuebleId == inmueble.Id && 
                c.Estado == EstadoContrato.Reservado);

            if (contratoReservado != null)
            {
                return "Reservado";
            }

            // Si no tiene contratos activos ni futuros, está disponible
            return "Disponible";
        }
    }
}
