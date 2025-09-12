using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Attributes;
using System.Text.Json;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeEmpleado]
    public class InquilinosController : Controller
    {
        private readonly IRepository<Inquilino> _repository;

        public InquilinosController(IRepository<Inquilino> repository)
        {
            _repository = repository;
        }

        // GET: Inquilinos
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // POST: Inquilinos/GetInquilinosData - DataTables AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> GetInquilinosData([FromBody] JsonElement request)
        {
            try
            {
                var inquilinos = await _repository.GetAllAsync();
                
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

                var allInquilinos = inquilinos.ToList();
                var totalRecords = allInquilinos.Count;
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    allInquilinos = allInquilinos.Where(i => 
                        i.Nombre.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        i.Apellido.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        i.Dni.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        i.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        (i.Telefono != null && i.Telefono.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Direccion != null && i.Direccion.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                var filteredRecords = allInquilinos.Count;

                // Apply sorting
                switch (orderColumn)
                {
                    case 0: // DNI
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.Dni).ToList()
                            : allInquilinos.OrderByDescending(i => i.Dni).ToList();
                        break;
                    case 1: // Nombre Completo
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.NombreCompleto).ToList()
                            : allInquilinos.OrderByDescending(i => i.NombreCompleto).ToList();
                        break;
                    case 2: // Email
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.Email).ToList()
                            : allInquilinos.OrderByDescending(i => i.Email).ToList();
                        break;
                    case 3: // Telefono
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.Telefono ?? "").ToList()
                            : allInquilinos.OrderByDescending(i => i.Telefono ?? "").ToList();
                        break;
                    case 4: // Direccion
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.Direccion ?? "").ToList()
                            : allInquilinos.OrderByDescending(i => i.Direccion ?? "").ToList();
                        break;
                    case 5: // FechaCreacion
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.FechaCreacion).ToList()
                            : allInquilinos.OrderByDescending(i => i.FechaCreacion).ToList();
                        break;
                    case 6: // Estado
                        allInquilinos = orderDirection == "asc" 
                            ? allInquilinos.OrderBy(i => i.Estado).ToList()
                            : allInquilinos.OrderByDescending(i => i.Estado).ToList();
                        break;
                    default:
                        allInquilinos = allInquilinos.OrderByDescending(i => i.FechaCreacion).ToList();
                        break;
                }
                
                // Apply pagination
                var pagedData = allInquilinos
                    .Skip(start)
                    .Take(length)
                    .Select(i => new
                    {
                        id = i.Id,
                        dni = i.Dni,
                        nombreCompleto = i.NombreCompleto,
                        email = i.Email,
                        telefono = i.Telefono ?? "",
                        direccion = i.Direccion ?? "",
                        fechaCreacion = i.FechaCreacion,
                        estado = i.Estado
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

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var Inquilino = await _repository.GetByIdAsync(id.Value);
            if (Inquilino == null)
            {
                return NotFound();
            }
            
            return View(Inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Inquilinos/FormModal - Returns unified modal for create/edit
        public async Task<IActionResult> FormModal(int? id)
        {
            Inquilino inquilino;
            
            if (id.HasValue)
            {
                // Editar - cargar inquilino existente
                var existingInquilino = await _repository.GetByIdAsync(id.Value);
                if (existingInquilino == null)
                {
                    return NotFound();
                }
                inquilino = existingInquilino;
            }
            else
            {
                // Crear - nuevo inquilino
                inquilino = new Inquilino();
            }
            
            return PartialView("_FormModal", inquilino);
        }

        // POST: Inquilinos/Save - Unified save action for create/edit
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] Inquilino inquilino)
        {
            Console.WriteLine($"Save method called with Id: {inquilino?.Id}, Nombre: {inquilino?.Nombre}");
            
            if (inquilino == null)
            {
                return Json(new { success = false, message = "Datos del inquilino no válidos." });
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    if (inquilino.Id == 0)
                    {
                        // Crear nuevo
                        await _repository.CreateAsync(inquilino);
                        return Json(new { success = true, message = "Inquilino creado exitosamente." });
                    }
                    else
                    {
                        // Actualizar existente
                        await _repository.UpdateAsync(inquilino);
                        return Json(new { success = true, message = "Inquilino actualizado exitosamente." });
                    }
                }
                catch (Exception ex)
                {
                    string action = inquilino.Id == 0 ? "crear" : "actualizar";
                    return Json(new { success = false, message = $"Error al {action} el inquilino: {ex.Message}" });
                }
            }

            var errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return Json(new { success = false, errors = errors });
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var Inquilino = await _repository.GetByIdAsync(id.Value);
            if (Inquilino == null)
            {
                return NotFound();
            }
            
            return View(Inquilino);
        }


        // POST: Inquilinos/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(Inquilino Inquilino)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(Inquilino);
                    return Json(new { success = true, message = "Inquilino actualizado exitosamente." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error al actualizar el inquilino: " + ex.Message });
                }
            }

            var errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return Json(new { success = false, errors = errors });
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var Inquilino = await _repository.GetByIdAsync(id.Value);
            if (Inquilino == null)
            {
                return NotFound();
            }

            return View(Inquilino);
        }

        // GET: Inquilinos/DetailsModal/5 - Returns partial view for modal
        public async Task<IActionResult> DetailsModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null)
            {
                return NotFound();
            }
            
            return PartialView("_DetailsModal", inquilino);
        }

        // GET: Inquilinos/DeleteModal/5 - Returns partial view for modal
        public async Task<IActionResult> DeleteModal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var inquilino = await _repository.GetByIdAsync(id.Value);
            if (inquilino == null)
            {
                return NotFound();
            }

            return PartialView("_DeleteModal", inquilino);
        }

        // POST: Inquilinos/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(Inquilino inquilino)
        {
            try
            {
                await _repository.DeleteAsync(inquilino.Id);
                return Json(new { success = true, message = "Inquilino eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el inquilino: " + ex.Message });
            }
        }
    }
}
