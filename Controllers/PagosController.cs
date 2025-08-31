using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class PagosController : Controller
    {
        private readonly IRepository<Pago> _pagoRepository;
        private readonly IRepository<Contrato> _contratoRepository;

        public PagosController(IRepository<Pago> pagoRepository, IRepository<Contrato> contratoRepository)
        {
            _pagoRepository = pagoRepository;
            _contratoRepository = contratoRepository;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            try
            {
                var pagos = await _pagoRepository.GetAllAsync();
                return View(pagos);
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

                await LoadContratosViewBag();
                return View(pago);
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
        public async Task<IActionResult> Edit(int id, Pago pago)
        {
            if (id != pago.Id)
            {
                TempData["Error"] = "ID de pago no válido";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var success = await _pagoRepository.UpdateAsync(pago);
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

                await LoadContratosViewBag();
                return View(pago);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el pago: {ex.Message}";
                await LoadContratosViewBag();
                return View(pago);
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
