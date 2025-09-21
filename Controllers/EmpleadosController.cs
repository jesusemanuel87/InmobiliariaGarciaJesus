using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;
using System.Text.Json;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeAdmin]
    public class EmpleadosController : Controller
    {
        private readonly EmpleadoService _empleadoService;
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<EmpleadosController> _logger;

        public EmpleadosController(
            EmpleadoService empleadoService,
            UsuarioService usuarioService,
            ILogger<EmpleadosController> logger)
        {
            _empleadoService = empleadoService;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public IActionResult Index(bool? estado = true, string? rol = null, string? buscar = null)
        {
            // Pasar datos para mantener los filtros
            ViewBag.EstadoSeleccionado = estado;
            ViewBag.RolSeleccionado = rol;
            ViewBag.BuscarSeleccionado = buscar;

            return View();
        }

        // POST: Empleados/GetEmpleadosData - DataTables AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> GetEmpleadosData([FromBody] JsonElement request)
        {
            try
            {
                var empleados = await _empleadoService.GetAllEmpleadosAsync();
                
                // Parse DataTables parameters
                int draw = request.TryGetProperty("draw", out var drawProp) ? drawProp.GetInt32() : 0;
                int start = request.TryGetProperty("start", out var startProp) ? startProp.GetInt32() : 0;
                int length = request.TryGetProperty("length", out var lengthProp) ? lengthProp.GetInt32() : 10;
                
                string searchValue = "";
                if (request.TryGetProperty("search", out var searchProp) && 
                    searchProp.TryGetProperty("value", out var searchValueProp))
                {
                    searchValue = searchValueProp.GetString() ?? "";
                }

                // Parse custom filters
                string estadoFilter = request.TryGetProperty("estado", out var estadoProp) ? estadoProp.GetString() ?? "" : "";
                string rolFilter = request.TryGetProperty("rol", out var rolProp) ? rolProp.GetString() ?? "" : "";
                string buscarFilter = request.TryGetProperty("buscar", out var buscarProp) ? buscarProp.GetString() ?? "" : "";

                // Apply filters
                var empleadosFiltrados = empleados.AsQueryable();

                // Estado filter
                if (!string.IsNullOrEmpty(estadoFilter) && bool.TryParse(estadoFilter, out bool estadoBool))
                {
                    empleadosFiltrados = empleadosFiltrados.Where(e => e.Estado == estadoBool);
                }

                // Rol filter
                if (!string.IsNullOrEmpty(rolFilter) && Enum.TryParse<RolEmpleado>(rolFilter, out var rolEnum))
                {
                    empleadosFiltrados = empleadosFiltrados.Where(e => e.Rol == rolEnum);
                }

                // Buscar filter (custom search)
                if (!string.IsNullOrEmpty(buscarFilter))
                {
                    empleadosFiltrados = empleadosFiltrados.Where(e =>
                        e.NombreCompleto.Contains(buscarFilter, StringComparison.OrdinalIgnoreCase) ||
                        e.Dni.Contains(buscarFilter, StringComparison.OrdinalIgnoreCase) ||
                        e.Email.Contains(buscarFilter, StringComparison.OrdinalIgnoreCase));
                }

                // DataTables global search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    empleadosFiltrados = empleadosFiltrados.Where(e =>
                        e.NombreCompleto.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        e.Dni.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        e.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        e.Rol.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase));
                }

                var totalRecords = empleados.Count();
                var filteredRecords = empleadosFiltrados.Count();

                // Apply pagination
                var empleadosPaginados = empleadosFiltrados
                    .Skip(start)
                    .Take(length)
                    .Select(e => new
                    {
                        id = e.Id,
                        dni = e.Dni,
                        nombreCompleto = e.NombreCompleto,
                        email = e.Email,
                        telefono = e.Telefono,
                        rol = e.Rol.ToString(),
                        estado = e.Estado,
                        fechaCreacion = e.FechaCreacion
                    })
                    .ToList();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = empleadosPaginados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos de empleados para DataTables");
                return Json(new { error = "Error al cargar los datos" });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);
                if (empleado == null)
                {
                    TempData["ErrorMessage"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del empleado: {Id}", id);
                TempData["ErrorMessage"] = "Error al cargar los detalles del empleado";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Empleado empleado, string nombreUsuario, string password)
        {
            if (!ModelState.IsValid)
            {
                return View(empleado);
            }

            if (string.IsNullOrEmpty(nombreUsuario) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "El nombre de usuario y contraseña son obligatorios");
                return View(empleado);
            }

            try
            {
                var (success, message) = await _usuarioService.CreateEmpleadoUsuarioAsync(empleado, nombreUsuario, password);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(empleado);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empleado");
                ModelState.AddModelError(string.Empty, "Error interno del servidor");
                return View(empleado);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);
                if (empleado == null)
                {
                    TempData["ErrorMessage"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar empleado para edición: {Id}", id);
                TempData["ErrorMessage"] = "Error al cargar el empleado";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Empleado empleado)
        {
            if (id != empleado.Id)
            {
                TempData["ErrorMessage"] = "ID de empleado no válido";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(empleado);
            }

            try
            {
                var (success, message) = await _empleadoService.UpdateEmpleadoAsync(empleado);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(empleado);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empleado: {Id}", id);
                ModelState.AddModelError(string.Empty, "Error interno del servidor");
                return View(empleado);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);
                if (empleado == null)
                {
                    TempData["ErrorMessage"] = "Empleado no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar empleado para eliminación: {Id}", id);
                TempData["ErrorMessage"] = "Error al cargar el empleado";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, message) = await _empleadoService.DeleteEmpleadoAsync(id);

                if (success)
                {
                    TempData["SuccessMessage"] = message;
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empleado: {Id}", id);
                TempData["ErrorMessage"] = "Error interno del servidor";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Empleados/FormModal - Returns unified modal for create/edit
        public async Task<IActionResult> FormModal(int? id)
        {
            Empleado empleado;
            
            if (id.HasValue)
            {
                // Editar - cargar empleado existente
                var existingEmpleado = await _empleadoService.GetEmpleadoByIdAsync(id.Value);
                if (existingEmpleado == null)
                {
                    return NotFound();
                }
                empleado = existingEmpleado;
            }
            else
            {
                // Crear - nuevo empleado
                empleado = new Empleado();
            }
            
            return PartialView("_FormModal", empleado);
        }

        // POST: Empleados/Save - Unified save action for create/edit
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] EmpleadoSaveRequest request)
        {
            _logger.LogInformation("Save request received: {@Request}", request);
            
            if (request?.Empleado == null)
            {
                _logger.LogWarning("Request or Empleado is null");
                return Json(new { success = false, message = "Datos del empleado no válidos." });
            }

            var empleado = request.Empleado;
            
            if (ModelState.IsValid)
            {
                try
                {
                    if (empleado.Id == 0)
                    {
                        // Crear nuevo empleado con usuario
                        if (string.IsNullOrEmpty(request.NombreUsuario) || string.IsNullOrEmpty(request.Password))
                        {
                            return Json(new { success = false, message = "El nombre de usuario y contraseña son obligatorios para crear un empleado." });
                        }

                        var (success, message) = await _usuarioService.CreateEmpleadoUsuarioAsync(empleado, request.NombreUsuario, request.Password);
                        
                        if (success)
                        {
                            return Json(new { success = true, message = "Empleado creado exitosamente." });
                        }
                        else
                        {
                            return Json(new { success = false, message = message });
                        }
                    }
                    else
                    {
                        // Actualizar empleado existente
                        var (success, message) = await _empleadoService.UpdateEmpleadoAsync(empleado);
                        
                        if (success)
                        {
                            // TODO: Implementar actualización de credenciales de usuario
                            // Por ahora solo actualizamos el empleado
                            string finalMessage = "Empleado actualizado exitosamente.";
                            
                            if (!string.IsNullOrEmpty(request.NombreUsuario) || !string.IsNullOrEmpty(request.Password))
                            {
                                finalMessage += " Nota: La actualización de credenciales de usuario estará disponible próximamente.";
                            }
                            
                            return Json(new { success = true, message = finalMessage });
                        }
                        else
                        {
                            return Json(new { success = false, message = message });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al guardar empleado");
                    string action = empleado.Id == 0 ? "crear" : "actualizar";
                    return Json(new { success = false, message = $"Error al {action} el empleado: {ex.Message}" });
                }
            }

            var errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return Json(new { success = false, errors = errors });
        }

        // GET: Empleados/DetailsModal/5 - Returns partial view for modal
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var empleado = await _empleadoService.GetEmpleadoByIdAsync(id.Value);
            if (empleado == null)
            {
                return NotFound();
            }
            
            return PartialView("_DetailsModal", empleado);
        }

        // GET: Empleados/DeleteModal/5 - Returns partial view for modal
        public async Task<IActionResult> DeleteModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var empleado = await _empleadoService.GetEmpleadoByIdAsync(id.Value);
            if (empleado == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModal", empleado);
        }

        // POST: Empleados/Delete - AJAX delete action
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] EmpleadoDeleteRequest request)
        {
            try
            {
                var (success, message) = await _empleadoService.DeleteEmpleadoAsync(request.Id);
                
                if (success)
                {
                    return Json(new { success = true, message = "Empleado eliminado exitosamente." });
                }
                else
                {
                    return Json(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empleado: {Id}", request.Id);
                return Json(new { success = false, message = "Error al eliminar el empleado: " + ex.Message });
            }
        }

        // GET: Empleados/GetEmpleadoDetails/5 - Returns empleado details for row expansion
        [HttpGet]
        public async Task<IActionResult> GetEmpleadoDetails(int id)
        {
            try
            {
                var empleado = await _empleadoService.GetEmpleadoByIdAsync(id);
                if (empleado == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    id = empleado.Id,
                    dni = empleado.Dni,
                    nombre = empleado.Nombre,
                    apellido = empleado.Apellido,
                    email = empleado.Email,
                    telefono = empleado.Telefono,
                    rol = empleado.Rol.ToString(),
                    estado = empleado.Estado,
                    fechaCreacion = empleado.FechaCreacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles del empleado: {Id}", id);
                return Json(new { error = "Error al cargar los detalles" });
            }
        }

        // Métodos AJAX para validaciones
        [HttpPost]
        public async Task<IActionResult> CheckEmailExists(string email, int? id = null)
        {
            try
            {
                var exists = await _empleadoService.EmailExistsAsync(email, id);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar email: {Email}", email);
                return Json(new { exists = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckDniExists(string dni, int? id = null)
        {
            try
            {
                var exists = await _empleadoService.DniExistsAsync(dni, id);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar DNI: {Dni}", dni);
                return Json(new { exists = false });
            }
        }
    }

    // Request models for AJAX operations
    public class EmpleadoSaveRequest
    {
        public Empleado Empleado { get; set; } = null!;
        public string? NombreUsuario { get; set; }
        public string? Password { get; set; }
    }

    public class EmpleadoDeleteRequest
    {
        public int Id { get; set; }
    }
}
