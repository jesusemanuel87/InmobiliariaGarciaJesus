using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Attributes;
using System.Text.Json;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeAdmin]
    public class TiposInmuebleController : Controller
    {
        private readonly TipoInmuebleRepository _tipoInmuebleRepository;
        private readonly ILogger<TiposInmuebleController> _logger;

        public TiposInmuebleController(TipoInmuebleRepository tipoInmuebleRepository, ILogger<TiposInmuebleController> logger)
        {
            _tipoInmuebleRepository = tipoInmuebleRepository;
            _logger = logger;
        }

        // Modelos de request para operaciones AJAX
        public class TipoInmuebleSaveRequest
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public bool Estado { get; set; } = true;
        }

        public class TipoInmuebleDeleteRequest
        {
            public int Id { get; set; }
        }

        // GET: TiposInmueble - Vista principal con DataTables
        public IActionResult Index()
        {
            return View();
        }

        // AJAX: Obtener datos para DataTables
        [HttpGet]
        public async Task<IActionResult> GetTiposInmuebleData()
        {
            try
            {
                var tipos = await _tipoInmuebleRepository.GetAllAsync();
                
                var data = tipos.Select(t => new
                {
                    id = t.Id,
                    nombre = t.Nombre,
                    descripcion = t.Descripcion ?? "",
                    estado = t.Estado,
                    estadoTexto = t.Estado ? "Activo" : "Inactivo",
                    fechaCreacion = t.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                    acciones = t.Id // Para generar botones de acción
                });

                return Json(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de tipos de inmueble");
                return Json(new { data = new List<object>() });
            }
        }

        // AJAX: Modal de formulario (Create/Edit)
        [HttpGet]
        public async Task<IActionResult> FormModal(int? id)
        {
            try
            {
                TipoInmuebleEntity tipo;
                
                if (id.HasValue)
                {
                    // Modo edición
                    tipo = await _tipoInmuebleRepository.GetByIdAsync(id.Value);
                    if (tipo == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    // Modo creación
                    tipo = new TipoInmuebleEntity { Estado = true };
                }

                ViewBag.IsEdit = id.HasValue;
                return PartialView("_FormModal", tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de formulario");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // AJAX: Modal de detalles
        [HttpGet]
        public async Task<IActionResult> DetailsModal(int id)
        {
            try
            {
                var tipo = await _tipoInmuebleRepository.GetByIdAsync(id);
                if (tipo == null)
                {
                    return NotFound();
                }
                return PartialView("_DetailsModal", tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de detalles");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // AJAX: Modal de eliminación
        [HttpGet]
        public async Task<IActionResult> DeleteModal(int id)
        {
            try
            {
                var tipo = await _tipoInmuebleRepository.GetByIdAsync(id);
                if (tipo == null)
                {
                    return NotFound();
                }
                return PartialView("_DeleteModal", tipo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de eliminación");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        // AJAX: Guardar tipo (Create/Update)
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TipoInmuebleSaveRequest request)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return Json(new { success = false, message = "El nombre es obligatorio." });
                }

                // Validar que el nombre no exista (excluyendo el actual si es edición)
                var existeNombre = await _tipoInmuebleRepository.ExisteNombreAsync(request.Nombre, request.Id > 0 ? request.Id : null);
                if (existeNombre)
                {
                    return Json(new { success = false, message = "Ya existe un tipo con este nombre." });
                }

                if (request.Id > 0)
                {
                    // Actualizar
                    var tipoExistente = await _tipoInmuebleRepository.GetByIdAsync(request.Id);
                    if (tipoExistente == null)
                    {
                        return Json(new { success = false, message = "Tipo no encontrado." });
                    }

                    tipoExistente.Nombre = request.Nombre.Trim();
                    tipoExistente.Descripcion = request.Descripcion?.Trim();
                    tipoExistente.Estado = request.Estado;

                    var updated = await _tipoInmuebleRepository.UpdateAsync(tipoExistente);
                    if (updated)
                    {
                        return Json(new { success = true, message = "Tipo actualizado exitosamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Error al actualizar el tipo." });
                    }
                }
                else
                {
                    // Crear
                    var nuevoTipo = new TipoInmuebleEntity
                    {
                        Nombre = request.Nombre.Trim(),
                        Descripcion = request.Descripcion?.Trim(),
                        Estado = request.Estado,
                        FechaCreacion = DateTime.Now
                    };

                    var id = await _tipoInmuebleRepository.CreateAsync(nuevoTipo);
                    if (id > 0)
                    {
                        return Json(new { success = true, message = "Tipo creado exitosamente." });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Error al crear el tipo." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar tipo de inmueble");
                return Json(new { success = false, message = "Error interno del servidor." });
            }
        }

        // AJAX: Eliminar tipo
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] TipoInmuebleDeleteRequest request)
        {
            try
            {
                var success = await _tipoInmuebleRepository.DeleteAsync(request.Id);
                if (success)
                {
                    return Json(new { success = true, message = "Tipo eliminado exitosamente." });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el tipo." });
                }
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo {TipoId}", request.Id);
                return Json(new { success = false, message = "Error interno del servidor." });
            }
        }

        // API: Obtener tipos activos para dropdowns
        [HttpGet]
        public async Task<IActionResult> GetTiposActivos()
        {
            try
            {
                var tipos = await _tipoInmuebleRepository.GetActivosAsync();
                var tiposDto = tipos.Select(t => new { id = t.Id, nombre = t.Nombre }).ToList();
                return Json(tiposDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos activos");
                return Json(new List<object>());
            }
        }

        // API: Validar nombre único
        [HttpPost]
        public async Task<IActionResult> ValidarNombre(string nombre, int? id)
        {
            try
            {
                var existe = await _tipoInmuebleRepository.ExisteNombreAsync(nombre, id);
                return Json(!existe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar nombre de tipo");
                return Json(false);
            }
        }
    }
}
