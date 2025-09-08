using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario, RolUsuario.Inquilino)]
    public class PagosController : Controller
    {
        private readonly IRepository<Pago> _pagoRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IPagoService _pagoService;

        public PagosController(IRepository<Pago> pagoRepository, IRepository<Contrato> contratoRepository, IPagoService pagoService)
        {
            _pagoRepository = pagoRepository;
            _contratoRepository = contratoRepository;
            _pagoService = pagoService;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            try
            {
                // Actualizar estados automáticamente antes de mostrar
                await _pagoService.ActualizarEstadosPagosAsync();
                
                // Esperar un momento para asegurar que los cambios se persistan
                await Task.Delay(100);
                
                // Forzar recarga desde la base de datos después de actualizar
                var pagos = await _pagoRepository.GetAllAsync();
                
                // Debug: Log para verificar multas en la vista
                foreach (var pago in pagos.Where(p => p.Multas > 0))
                {
                    System.Diagnostics.Debug.WriteLine($"Vista Pagos - Pago {pago.Id}: Multa ${pago.Multas}");
                }
                
                // Ordenar por fecha de vencimiento para consistencia
                var pagosOrdenados = pagos.OrderBy(p => p.FechaVencimiento).ToList();
                
                return View(pagosOrdenados);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar los pagos: {ex.Message}";
                return View(new List<Pago>());
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
                        Console.WriteLine($"[DEBUG EDIT] Pago {id} marcado como pagado, preservando multas: {pagoOriginal.Multas}");
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

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _pagoRepository.DeleteAsync(id);
                if (success)
                {
                    TempData["Success"] = "Pago eliminado exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el pago";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el pago: {ex.Message}";
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
                Console.WriteLine($"[DEBUG MODAL] Pago {id} - Importe: {pago.Importe}, Intereses: {pago.Intereses}, Multas: {pago.Multas}, Total: {pago.TotalAPagar}");
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
            ViewBag.Contratos = contratos.Select(c => new { 
                Value = c.Id, 
                Text = $"Contrato #{c.Id} - {c.Estado}" 
            }).ToList();
        }
    }
}
