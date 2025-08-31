using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class ContratosController : Controller
    {
        private readonly IContratoService _contratoService;
        private readonly IRepository<Inquilino> _inquilinoRepository;
        private readonly IRepository<Inmueble> _inmuebleRepository;
        private readonly IPagoService _pagoService;

        public ContratosController(IContratoService contratoService,
                                  IRepository<Inquilino> inquilinoRepository,
                                  IRepository<Inmueble> inmuebleRepository,
                                  IPagoService pagoService)
        {
            _contratoService = contratoService;
            _inquilinoRepository = inquilinoRepository;
            _inmuebleRepository = inmuebleRepository;
            _pagoService = pagoService;
        }

        // GET: Contratos
        public async Task<IActionResult> Index()
        {
            try
            {
                var contratos = await _contratoService.GetAllAsync();
                return View(contratos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los contratos: " + ex.Message;
                return View(new List<Contrato>());
            }
        }

        // GET: Contratos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contrato = await _contratoService.GetByIdAsync(id.Value);
                if (contrato == null)
                {
                    return NotFound();
                }

                return View(contrato);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el contrato: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contratos/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var inquilinos = await _inquilinoRepository.GetAllAsync();
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                
                ViewBag.Inquilinos = inquilinos;
                ViewBag.Inmuebles = inmuebles;
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FechaInicio,FechaFin,Precio,InquilinoId,InmuebleId")] Contrato contrato)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var (isValid, errors) = await _contratoService.ValidateContratoAsync(contrato, false);
                    
                    if (!isValid)
                    {
                        foreach (var error in errors)
                        {
                            ModelState.AddModelError(error.Key, error.Value);
                        }
                    }
                    else
                    {
                        await _contratoService.CreateContratoAsync(contrato);
                        
                        // Generar plan de pagos automáticamente
                        await _pagoService.GenerarPlanPagosAsync(contrato.Id);
                        
                        TempData["Success"] = "Contrato creado exitosamente y plan de pagos generado.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al crear el contrato: " + ex.Message;
                }
            }

            var inquilinos = await _inquilinoRepository.GetAllAsync();
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            ViewBag.Inquilinos = inquilinos;
            ViewBag.Inmuebles = inmuebles;
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contrato = await _contratoService.GetByIdAsync(id.Value);
                if (contrato == null)
                {
                    return NotFound();
                }

                var inquilinos = await _inquilinoRepository.GetAllAsync();
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                ViewBag.Inquilinos = inquilinos;
                ViewBag.Inmuebles = inmuebles;
                
                return View(contrato);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el contrato: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FechaInicio,FechaFin,Precio,InquilinoId,InmuebleId,Estado,FechaCreacion")] Contrato contrato)
        {
            if (id != contrato.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var (isValid, errors) = await _contratoService.ValidateContratoAsync(contrato, true);
                    
                    if (!isValid)
                    {
                        foreach (var error in errors)
                        {
                            ModelState.AddModelError(error.Key, error.Value);
                        }
                    }
                    else
                    {
                        await _contratoService.UpdateContratoAsync(contrato);
                        TempData["Success"] = "Contrato actualizado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al actualizar el contrato: " + ex.Message;
                }
            }

            var inquilinos = await _inquilinoRepository.GetAllAsync();
            var inmuebles = await _inmuebleRepository.GetAllAsync();
            ViewBag.Inquilinos = inquilinos;
            ViewBag.Inmuebles = inmuebles;
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var contrato = await _contratoService.GetByIdAsync(id.Value);
                if (contrato == null)
                {
                    return NotFound();
                }

                return View(contrato);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el contrato: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _contratoService.DeleteContratoAsync(id);
                TempData["Success"] = "Contrato cancelado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cancelar el contrato: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
