using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;
using System.Text.Json;
using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario, RolUsuario.Inquilino)]
    public class PagosController : Controller
    {
        private readonly PagoRepository _pagoRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IPagoService _pagoService;
        private readonly UsuarioRepository _usuarioRepository;

        public PagosController(PagoRepository pagoRepository, IRepository<Contrato> contratoRepository, IPagoService pagoService, UsuarioRepository usuarioRepository)
        {
            _pagoRepository = pagoRepository;
            _contratoRepository = contratoRepository;
            _pagoService = pagoService;
            _usuarioRepository = usuarioRepository;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            try
            {
                // Actualizar estados automáticamente antes de mostrar
                await _pagoService.ActualizarEstadosPagosAsync();
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los pagos: {ex.Message}";
                return View();
            }
        }

        // POST: Pagos/GetPagosData - DataTables AJAX endpoint
        [HttpPost]
        public async Task<IActionResult> GetPagosData([FromBody] JsonElement request)
        {
            try
            {
                // Update payment states before loading data
                await _pagoService.ActualizarEstadosPagosAsync();
                
                var pagosWithRelatedData = await _pagoRepository.GetAllWithRelatedDataAsync();
                
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
                string filtroEstado = request.TryGetProperty("filtroEstado", out var estadoProp) ? estadoProp.GetString() ?? "" : "";
                string filtroEstadoContrato = request.TryGetProperty("filtroEstadoContrato", out var estadoContratoProp) ? estadoContratoProp.GetString() ?? "" : "";
                string filtroNumeroContrato = request.TryGetProperty("filtroNumeroContrato", out var numeroContratoProp) ? numeroContratoProp.GetString() ?? "" : "";
                string filtroMes = request.TryGetProperty("filtroMes", out var mesProp) ? mesProp.GetString() ?? "" : "";
                string filtroAnio = request.TryGetProperty("filtroAnio", out var anioProp) ? anioProp.GetString() ?? "" : "";
                string filtroMonto = request.TryGetProperty("filtroMonto", out var montoProp) ? montoProp.GetString() ?? "" : "";
                
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

                var allPagos = pagosWithRelatedData.ToList();
                var totalRecords = allPagos.Count;
                
                // Apply custom filters
                if (!string.IsNullOrEmpty(filtroEstado))
                {
                    allPagos = allPagos.Where(p => ((dynamic)p).Estado.ToString().Equals(filtroEstado, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                if (!string.IsNullOrEmpty(filtroEstadoContrato))
                {
                    allPagos = allPagos.Where(p => ((dynamic)p).EstadoContrato.ToString().Equals(filtroEstadoContrato, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                if (!string.IsNullOrEmpty(filtroNumeroContrato) && int.TryParse(filtroNumeroContrato, out int numeroContrato))
                {
                    allPagos = allPagos.Where(p => ((dynamic)p).ContratoId == numeroContrato).ToList();
                }
                
                if (!string.IsNullOrEmpty(filtroMes) || !string.IsNullOrEmpty(filtroAnio))
                {
                    allPagos = allPagos.Where(p => {
                        var fechaVencimiento = ((dynamic)p).FechaVencimiento;
                        if (fechaVencimiento != null)
                        {
                            var fecha = (DateTime)fechaVencimiento;
                            bool mesMatch = string.IsNullOrEmpty(filtroMes) || fecha.Month == int.Parse(filtroMes);
                            bool anioMatch = string.IsNullOrEmpty(filtroAnio) || fecha.Year == int.Parse(filtroAnio);
                            return mesMatch && anioMatch;
                        }
                        return false;
                    }).ToList();
                }
                
                if (!string.IsNullOrEmpty(filtroMonto))
                {
                    var rangoParts = filtroMonto.Split('-');
                    if (rangoParts.Length == 2 && 
                        decimal.TryParse(rangoParts[0], out decimal minMonto) && 
                        decimal.TryParse(rangoParts[1], out decimal maxMonto))
                    {
                        allPagos = allPagos.Where(p => {
                            var importe = ((dynamic)p).Importe;
                            if (importe != null)
                            {
                                var monto = (decimal)importe;
                                return monto >= minMonto && monto <= maxMonto;
                            }
                            return false;
                        }).ToList();
                    }
                }
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    allPagos = allPagos.Where(p => 
                        ((dynamic)p).ContratoId.ToString().Contains(searchValue) ||
                        ((dynamic)p).Estado.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                        (((dynamic)p).InquilinoNombre?.ToString()?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true) ||
                        (((dynamic)p).InmuebleDireccion?.ToString()?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
                    ).ToList();
                }

                var filteredRecords = allPagos.Count;

                // Apply sorting
                switch (orderColumn)
                {
                    case 0: // ContratoId
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).ContratoId).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).ContratoId).ToList();
                        break;
                    case 1: // Inquilino
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).InquilinoNombre).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).InquilinoNombre).ToList();
                        break;
                    case 2: // Inmueble
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).InmuebleDireccion).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).InmuebleDireccion).ToList();
                        break;
                    case 3: // FechaVencimiento
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).FechaVencimiento).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).FechaVencimiento).ToList();
                        break;
                    case 4: // Importe
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).Importe).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).Importe).ToList();
                        break;
                    case 5: // Multas
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).Multas).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).Multas).ToList();
                        break;
                    case 6: // Intereses
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).Intereses).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).Intereses).ToList();
                        break;
                    case 7: // Total (calculated)
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).Importe + ((dynamic)p).Multas + ((dynamic)p).Intereses).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).Importe + ((dynamic)p).Multas + ((dynamic)p).Intereses).ToList();
                        break;
                    case 8: // Estado
                        allPagos = orderDirection == "asc" 
                            ? allPagos.OrderBy(p => ((dynamic)p).Estado).ToList()
                            : allPagos.OrderByDescending(p => ((dynamic)p).Estado).ToList();
                        break;
                    default:
                        allPagos = allPagos.OrderBy(p => ((dynamic)p).FechaVencimiento).ToList();
                        break;
                }
                
                // Apply pagination
                var pagedData = allPagos
                    .Skip(start)
                    .Take(length)
                    .Select(p => new
                    {
                        id = ((dynamic)p).Id,
                        contratoId = ((dynamic)p).ContratoId,
                        inquilinoNombre = ((dynamic)p).InquilinoNombre ?? "Sin inquilino",
                        inmuebleDireccion = ((dynamic)p).InmuebleDireccion ?? "Sin dirección",
                        fechaVencimiento = ((dynamic)p).FechaVencimiento,
                        monto = ((dynamic)p).Importe,
                        multas = ((dynamic)p).Multas,
                        intereses = ((dynamic)p).Intereses,
                        estado = ((dynamic)p).Estado.ToString()
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

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el pago: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Pagos/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadContratosViewBag();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar la página de creación: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    pago.FechaCreacion = DateTime.Now;
                    await _pagoRepository.CreateAsync(pago);
                    TempData["Success"] = "Pago creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                await LoadContratosViewBag();
                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el pago: {ex.Message}";
                await LoadContratosViewBag();
                return View(pago);
            }
        }

        // GET: Pagos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener información del contrato para mostrar
                var contrato = await _contratoRepository.GetByIdAsync(pago.ContratoId);
                var contratoInfo = contrato != null ? 
                    $"Contrato #{contrato.Id} - {contrato.Inquilino?.NombreCompleto}" : 
                    $"Contrato #{pago.ContratoId}";

                var viewModel = new PagoEditViewModel
                {
                    Id = pago.Id,
                    Numero = pago.Numero,
                    ContratoId = pago.ContratoId,
                    Importe = pago.Importe,
                    Intereses = pago.Intereses,
                    Multas = pago.Multas,
                    TotalAPagar = pago.TotalAPagar,
                    FechaPago = pago.FechaPago,
                    Estado = pago.Estado,
                    MetodoPago = pago.MetodoPago,
                    Observaciones = pago.Observaciones,
                    FechaCreacion = pago.FechaCreacion,
                    ContratoInfo = contratoInfo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el pago: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PagoEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                TempData["Error"] = "ID de pago no válido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Obtener el pago original para preservar campos no editables
                    var pagoOriginal = await _pagoRepository.GetByIdAsync(id);
                    if (pagoOriginal == null)
                    {
                        TempData["Error"] = "Pago no encontrado";
                        return RedirectToAction(nameof(Index));
                    }

                    // Solo actualizar campos editables
                    pagoOriginal.Estado = viewModel.Estado;
                    pagoOriginal.MetodoPago = viewModel.MetodoPago;
                    pagoOriginal.Observaciones = viewModel.Observaciones;

                    // Si se marca como pagado, establecer fecha de pago
                    if (viewModel.Estado == EstadoPago.Pagado && !pagoOriginal.FechaPago.HasValue)
                    {
                        pagoOriginal.FechaPago = DateTime.Today;
                        // NO calcular intereses/multas para preservar multas por finalización temprana
                    }

                    var success = await _pagoRepository.UpdateAsync(pagoOriginal);
                    if (success)
                    {
                        TempData["Success"] = "Pago actualizado exitosamente";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo actualizar el pago";
                    }
                }

                // Recalcular información del contrato para la vista
                var contrato = await _contratoRepository.GetByIdAsync(viewModel.ContratoId);
                viewModel.ContratoInfo = contrato != null ? 
                    $"Contrato #{contrato.Id} - {contrato.Inquilino?.NombreCompleto}" : 
                    $"Contrato #{viewModel.ContratoId}";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el pago: {ex.Message}";
                return View(viewModel);
            }
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar el pago: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        // GET: Pagos/ByContrato/5
        public async Task<IActionResult> ByContrato(int contratoId)
        {
            try
            {
                if (_pagoRepository is PagoRepository pagoRepo)
                {
                    var pagos = await pagoRepo.GetPagosByContratoIdAsync(contratoId);
                    ViewBag.ContratoId = contratoId;
                    return View("Index", pagos);
                }
                
                TempData["Error"] = "Funcionalidad no disponible";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los pagos del contrato: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Pagos/RegistrarPago/5
        public async Task<IActionResult> RegistrarPago(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                if (pago.Estado == EstadoPago.Pagado)
                {
                    return Json(new { success = false, message = "El pago ya está registrado" });
                }

                // NO calcular intereses/multas para evitar sobrescribir multas por finalización temprana
                // await _pagoService.CalcularInteresesYMultasAsync(id);
                
                // Usar el pago tal como está en la base de datos
                if (pago == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado después de actualizar" });
                }

                // Obtener información del contrato para mostrar
                var contrato = await _contratoRepository.GetByIdAsync(pago.ContratoId);
                var contratoInfo = contrato != null ? 
                    $"Contrato #{contrato.Id} - {contrato.Inquilino?.NombreCompleto ?? "Sin inquilino"}" : 
                    $"Contrato #{pago.ContratoId}";

                var viewModel = new PagoRegistroViewModel
                {
                    Id = pago.Id,
                    Numero = pago.Numero,
                    ContratoId = pago.ContratoId,
                    Importe = pago.Importe,
                    Intereses = pago.Intereses,
                    Multas = pago.Multas,
                    TotalAPagar = pago.TotalAPagar,
                    FechaVencimiento = pago.FechaVencimiento,
                    Estado = pago.Estado,
                    FechaCreacion = pago.FechaCreacion,
                    ContratoInfo = contratoInfo
                };

                return PartialView("_RegistrarPagoModal", viewModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al cargar el pago: {ex.Message}" });
            }
        }

        // POST: Pagos/RegistrarPago/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(int id, PagoRegistroViewModel viewModel)
        {
            try
            {
                if (id != viewModel.Id)
                {
                    return Json(new { success = false, message = "ID de pago no válido" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    return Json(new { success = false, message = "Pago no encontrado" });
                }

                if (pago.Estado == EstadoPago.Pagado)
                {
                    return Json(new { success = false, message = "El pago ya está registrado" });
                }

                // Actualizar el pago con los datos del formulario
                pago.FechaPago = DateTime.Today;
                pago.Estado = EstadoPago.Pagado;
                pago.MetodoPago = viewModel.MetodoPago;
                pago.Observaciones = viewModel.Observaciones;
                
                await _pagoRepository.UpdateAsync(pago);

                return Json(new { 
                    success = true, 
                    message = "Pago registrado exitosamente",
                    intereses = pago.Intereses,
                    multas = pago.Multas,
                    totalAPagar = pago.TotalAPagar
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al registrar el pago: {ex.Message}" });
            }
        }

        private async Task LoadContratosViewBag()
        {
            var contratos = await _contratoRepository.GetAllAsync();
            ViewBag.Contratos = contratos.Select(c => new 
            {
                Id = c.Id,
                Display = $"Contrato #{c.Id} - {c.Inquilino?.NombreCompleto ?? "Sin inquilino"}"
            }).ToList();
        }

        // Modal endpoints for AJAX
        [HttpGet]
        public async Task<IActionResult> DetailsModal(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    return NotFound();
                }

                // Get related data
                var contrato = await _contratoRepository.GetByIdAsync(pago.ContratoId);
                ViewBag.ContratoInfo = contrato != null ? 
                    $"Contrato #{contrato.Id} - {contrato.Inquilino?.NombreCompleto}" : 
                    $"Contrato #{pago.ContratoId}";

                return PartialView("_DetailsModal", pago);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> FormModal(int? id)
        {
            try
            {
                Pago pago;
                if (id.HasValue)
                {
                    pago = await _pagoRepository.GetByIdAsync(id.Value);
                    if (pago == null)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    pago = new Pago();
                }

                await LoadContratosViewBag();
                return PartialView("_FormModal", pago);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteModal(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    return NotFound();
                }

                // Get related data
                var contrato = await _contratoRepository.GetByIdAsync(pago.ContratoId);
                ViewBag.ContratoInfo = contrato != null ? 
                    $"Contrato #{contrato.Id} - {contrato.Inquilino?.NombreCompleto}" : 
                    $"Contrato #{pago.ContratoId}";

                return PartialView("_DeleteModal", pago);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _pagoRepository.DeleteAsync(id);
                if (success)
                {
                    return Json(new { success = true, message = "Pago eliminado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el pago" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Pagos/AuditoriaModal/5
        [HttpGet]
        [AuthorizeRole(RolUsuario.Administrador)]
        public async Task<IActionResult> AuditoriaModal(int id)
        {
            try
            {
                var pago = await _pagoRepository.GetByIdAsync(id);
                if (pago == null)
                {
                    return NotFound();
                }

                // Obtener información de usuarios para auditoría
                Usuario? creadoPor = null;
                Usuario? anuladoPor = null;

                if (pago.CreadoPorId.HasValue)
                {
                    creadoPor = await _usuarioRepository.GetByIdAsync(pago.CreadoPorId.Value);
                }

                if (pago.AnuladoPorId.HasValue)
                {
                    anuladoPor = await _usuarioRepository.GetByIdAsync(pago.AnuladoPorId.Value);
                }

                // Crear ViewModel de auditoría
                var auditoriaViewModel = new PagoAuditoriaViewModel
                {
                    TipoEntidad = "Pago",
                    EntidadId = pago.Id,
                    NumeroPago = pago.Id,
                    ContratoId = pago.ContratoId,
                    Importe = pago.Importe,
                    MetodoPago = pago.MetodoPago?.ToString(),
                    FechaCreacion = pago.FechaCreacion,
                    CreadoPor = creadoPor?.NombreUsuario ?? "Sistema",
                    UsuarioCreador = creadoPor?.NombreUsuario,
                    FechaAnulacion = pago.FechaAnulacion,
                    AnuladoPor = anuladoPor?.NombreUsuario,
                    UsuarioAnulador = anuladoPor?.NombreUsuario,
                    EstadoActual = pago.Estado.ToString(),
                    AccionRealizada = pago.FechaAnulacion.HasValue ? "Anulación del Pago" : null,
                    FechaModificacion = pago.FechaAnulacion,
                    ModificadoPor = anuladoPor?.NombreUsuario,
                    UsuarioModificador = anuladoPor?.NombreUsuario,
                    Observaciones = !string.IsNullOrEmpty(pago.Observaciones) ? pago.Observaciones : $"Pago de ${pago.Importe:N0} para contrato #{pago.ContratoId}"
                };

                return PartialView("_PagoAuditoriaModal", auditoriaViewModel);
            }
            catch (Exception ex)
            {
                // Log error
                return BadRequest("Error al cargar información de auditoría");
            }
        }

        // GET: Pagos/MisPagos - Vista de pagos del inquilino logueado
        [AuthorizeMultipleRoles(RolUsuario.Inquilino)]
        public async Task<IActionResult> MisPagos()
        {
            try
            {
                // Obtener el usuario logueado usando AuthService
                var usuarioId = AuthService.GetUsuarioId(User);
                if (!usuarioId.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario?.InquilinoId == null)
                {
                    TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un inquilino.";
                    return RedirectToAction("Index", "Home");
                }

                // Actualizar estados automáticamente antes de mostrar
                await _pagoService.ActualizarEstadosPagosAsync();

                // Obtener contratos del inquilino
                var todosContratos = await _contratoRepository.GetAllAsync();
                var misContratos = todosContratos
                    .Where(c => c.InquilinoId == usuario.InquilinoId.Value)
                    .Select(c => c.Id)
                    .ToList();

                if (!misContratos.Any())
                {
                    ViewBag.SinPagos = true;
                    ViewBag.UserRole = "Inquilino";
                    ViewBag.EsMisPagos = true;
                    return View("Index");
                }

                // Pasar IDs de contratos para filtrar en GetPagosData
                ViewBag.MisContratosIds = string.Join(",", misContratos);
                ViewBag.UserRole = "Inquilino";
                ViewBag.EsMisPagos = true;
                
                return View("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar sus pagos: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Pagos/GetMisPagosData - DataTables AJAX endpoint para inquilinos
        [HttpPost]
        [AuthorizeMultipleRoles(RolUsuario.Inquilino)]
        public async Task<IActionResult> GetMisPagosData([FromBody] JsonElement request)
        {
            try
            {
                // Obtener el usuario logueado
                var usuarioId = AuthService.GetUsuarioId(User);
                if (!usuarioId.HasValue)
                {
                    return Json(new { error = "Usuario no autenticado" });
                }

                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario?.InquilinoId == null)
                {
                    return Json(new { error = "No tiene permisos" });
                }

                // Actualizar estados automáticamente
                await _pagoService.ActualizarEstadosPagosAsync();
                
                var pagosWithRelatedData = await _pagoRepository.GetAllWithRelatedDataAsync();
                
                // Filtrar solo pagos de contratos del inquilino
                var todosContratos = await _contratoRepository.GetAllAsync();
                var misContratosIds = todosContratos
                    .Where(c => c.InquilinoId == usuario.InquilinoId.Value)
                    .Select(c => c.Id)
                    .ToList();

                var misPagos = pagosWithRelatedData
                    .Where(p => misContratosIds.Contains(((dynamic)p).ContratoId))
                    .ToList();
                
                // Parse DataTables parameters (igual que GetPagosData)
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
                string filtroEstado = request.TryGetProperty("filtroEstado", out var estadoProp) ? estadoProp.GetString() ?? "" : "";
                
                var allPagos = misPagos.ToList();
                var totalRecords = allPagos.Count;
                
                // Apply filters
                if (!string.IsNullOrEmpty(filtroEstado))
                {
                    allPagos = allPagos.Where(p => ((dynamic)p).Estado.ToString().Equals(filtroEstado, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    allPagos = allPagos.Where(p =>
                        (((dynamic)p).ContratoId.ToString().Contains(searchValue)) ||
                        (((dynamic)p).Importe.ToString().Contains(searchValue)) ||
                        (((dynamic)p).InmuebleDireccion?.ToString()?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
                    ).ToList();
                }

                var filteredRecords = allPagos.Count;

                // Apply sorting (por defecto: fecha vencimiento descendente)
                allPagos = allPagos.OrderByDescending(p => ((dynamic)p).FechaVencimiento).ToList();
                
                // Apply pagination
                var pagedData = allPagos
                    .Skip(start)
                    .Take(length)
                    .Select(p => new
                    {
                        id = ((dynamic)p).Id,
                        contratoId = ((dynamic)p).ContratoId,
                        inquilinoNombre = ((dynamic)p).InquilinoNombre ?? "Sin inquilino",
                        inmuebleDireccion = ((dynamic)p).InmuebleDireccion ?? "Sin dirección",
                        fechaVencimiento = ((dynamic)p).FechaVencimiento,
                        monto = ((dynamic)p).Importe,
                        multas = ((dynamic)p).Multas,
                        intereses = ((dynamic)p).Intereses,
                        estado = ((dynamic)p).Estado.ToString()
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

    }
}
