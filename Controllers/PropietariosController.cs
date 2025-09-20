using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Attributes;
using System.Text.Json;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeEmpleado]
    public class PropietariosController : Controller
    {
        private readonly IRepository<Propietario> _repository;
        private readonly IRepository<Inmueble> _inmuebleRepository;

        public PropietariosController(IRepository<Propietario> repository, IRepository<Inmueble> inmuebleRepository)
        {
            _repository = repository;
            _inmuebleRepository = inmuebleRepository;
        }

        // GET: Propietarios
        public IActionResult Index()
        {
            // Pasar el rol del usuario a la vista
            ViewBag.UserRole = HttpContext.Session.GetString("UserRole");
            return View();
        }

        // POST: Propietarios/GetPropietariosData - DataTables AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> GetPropietariosData([FromBody] JsonElement request)
        {
            try
            {
                var propietarios = await _repository.GetAllAsync();
                
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
                
                // Get order parameters
                int orderColumn = 0;
                string orderDirection = "asc";
                if (request.TryGetProperty("order", out var orderProp) && orderProp.ValueKind == JsonValueKind.Array)
                {
                    var orderArray = orderProp.EnumerateArray().FirstOrDefault();
                    if (orderArray.ValueKind != JsonValueKind.Undefined)
                    {
                        orderColumn = orderArray.TryGetProperty("column", out var colProp) ? colProp.GetInt32() : 0;
                        orderDirection = orderArray.TryGetProperty("dir", out var dirProp) ? dirProp.GetString() ?? "asc" : "asc";
                    }
                }

                var allPropietarios = propietarios.ToList();
                var totalRecords = allPropietarios.Count;
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    allPropietarios = allPropietarios.Where(p => 
                        p.Nombre.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        p.Apellido.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        p.Dni.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        p.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        (p.Telefono != null && p.Telefono.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Direccion != null && p.Direccion.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var filteredRecords = allPropietarios.Count;

                // Apply sorting
                switch (orderColumn)
                {
                    case 0: // DNI
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.Dni).ToList()
                            : allPropietarios.OrderByDescending(p => p.Dni).ToList();
                        break;
                    case 1: // Nombre Completo
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.NombreCompleto).ToList()
                            : allPropietarios.OrderByDescending(p => p.NombreCompleto).ToList();
                        break;
                    case 2: // Email
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.Email).ToList()
                            : allPropietarios.OrderByDescending(p => p.Email).ToList();
                        break;
                    case 3: // Telefono
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.Telefono ?? "").ToList()
                            : allPropietarios.OrderByDescending(p => p.Telefono ?? "").ToList();
                        break;
                    case 4: // FechaCreacion
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.FechaCreacion).ToList()
                            : allPropietarios.OrderByDescending(p => p.FechaCreacion).ToList();
                        break;
                    case 5: // Estado
                        allPropietarios = orderDirection == "asc" 
                            ? allPropietarios.OrderBy(p => p.Estado).ToList()
                            : allPropietarios.OrderByDescending(p => p.Estado).ToList();
                        break;
                    default:
                        allPropietarios = allPropietarios.OrderByDescending(p => p.FechaCreacion).ToList();
                        break;
                }
                
                // Apply pagination and include inmuebles data
                var pagedData = allPropietarios
                    .Skip(start)
                    .Take(length)
                    .Select(p => new
                    {
                        id = p.Id,
                        dni = p.Dni,
                        nombreCompleto = p.NombreCompleto,
                        email = p.Email,
                        telefono = p.Telefono ?? "",
                        fechaCreacion = p.FechaCreacion,
                        estado = p.Estado,
                        hasInmuebles = ((InmuebleRepository)_inmuebleRepository).GetInmueblesByPropietarioIdAsync(p.Id).Result.Any(),
                        canDelete = HttpContext.Session.GetString("UserRole") == "Administrador"
                    })
                    .ToList();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }
            
            return View(propietario);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Propietarios/FormModal - Returns unified modal for create/edit
        public async Task<IActionResult> FormModal(int? id)
        {
            Propietario propietario;
            
            if (id.HasValue)
            {
                // Editar - cargar propietario existente
                var existingPropietario = await _repository.GetByIdAsync(id.Value);
                if (existingPropietario == null)
                {
                    return NotFound();
                }
                propietario = existingPropietario;
            }
            else
            {
                // Crear - nuevo propietario
                propietario = new Propietario();
            }
            
            return PartialView("_FormModal", propietario);
        }

        // POST: Propietarios/Save - Unified save action for create/edit
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Propietario propietario)
        {
            
            if (propietario == null)
            {
                return Json(new { success = false, message = "Datos del propietario no válidos." });
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    if (propietario.Id == 0)
                    {
                        // Crear nuevo
                        await _repository.CreateAsync(propietario);
                        return Json(new { success = true, message = "Propietario creado exitosamente." });
                    }
                    else
                    {
                        // Actualizar existente
                        await _repository.UpdateAsync(propietario);
                        return Json(new { success = true, message = "Propietario actualizado exitosamente." });
                    }
                }
                catch (Exception ex)
                {
                    string action = propietario.Id == 0 ? "crear" : "actualizar";
                    return Json(new { success = false, message = $"Error al {action} el propietario: {ex.Message}" });
                }
            }

            var errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return Json(new { success = false, errors = errors });
        }

        // POST: Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.CreateAsync(propietario);
                    TempData["Success"] = "Propietario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el propietario: " + ex.Message);
                }
            }
            return View(propietario);
        }

        // GET: Propietarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }
            
            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(propietario);
                    TempData["Success"] = "Propietario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el propietario: " + ex.Message);
                }
            }
            return View(propietario);
        }

        // GET: Propietarios/DetailsModal/5 - Returns partial view for modal
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }
            
            return PartialView("_DetailsModal", propietario);
        }

        // GET: Propietarios/DeleteModal/5 - Returns partial view for modal
        public async Task<IActionResult> DeleteModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModal", propietario);
        }

        // GET: Propietarios/Delete/5
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // POST: Propietarios/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(Propietario propietario)
        {
            try
            {
                await _repository.DeleteAsync(propietario.Id);
                return Json(new { success = true, message = "Propietario eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el propietario: " + ex.Message });
            }
        }

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                TempData["Success"] = "Propietario eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el propietario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Propietarios/Inmuebles/5
        public async Task<IActionResult> Inmuebles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _repository.GetByIdAsync(id.Value);
            if (propietario == null)
            {
                return NotFound();
            }

            var inmuebles = await ((InmuebleRepository)_inmuebleRepository).GetInmueblesByPropietarioIdAsync(id.Value);
            
            ViewBag.PropietarioNombre = propietario.NombreCompleto;
            ViewBag.PropietarioId = id.Value;
            
            return View(inmuebles);
        }

        // AJAX endpoint to get inmuebles for a propietario
        [HttpGet]
        public async Task<IActionResult> GetInmuebles(int id)
        {
            var inmuebles = await ((InmuebleRepository)_inmuebleRepository).GetInmueblesByPropietarioIdAsync(id);
            return PartialView("_InmueblesPartial", inmuebles);
        }

        // AJAX endpoint to get inmuebles data for DataTables expansion
        [HttpGet]
        public async Task<IActionResult> GetInmueblesData(int id)
        {
            try
            {
                var inmuebles = await ((InmuebleRepository)_inmuebleRepository).GetInmueblesByPropietarioIdAsync(id);
                
                var inmueblesData = inmuebles.Select(i => new
                {
                    direccion = i.Direccion,
                    tipo = i.Tipo.ToString(),
                    uso = i.Uso.ToString(),
                    estado = i.Estado.ToString(),
                    estadoContrato = i.EstadoContratoTexto,
                    estadoContratoCss = i.EstadoContratoCssClass,
                    precio = i.Precio ?? 0
                }).ToList();

                return Json(new { success = true, data = inmueblesData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
