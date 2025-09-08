using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;

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

        public async Task<IActionResult> Index()
        {
            try
            {
                var empleados = await _empleadoService.GetAllEmpleadosAsync();
                return View(empleados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de empleados");
                TempData["ErrorMessage"] = "Error al cargar los empleados";
                return View(new List<Empleado>());
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
}
