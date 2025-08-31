using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Controllers
{
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
            var Inquilinos = await _repository.GetAllAsync();
            return View(Inquilinos);
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

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inquilino Inquilino)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.CreateAsync(Inquilino);
                    TempData["Success"] = "Inquilino creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el Inquilino: " + ex.Message);
                }
            }
            return View(Inquilino);
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

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inquilino Inquilino)
        {
            if (id != Inquilino.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _repository.UpdateAsync(Inquilino);
                    TempData["Success"] = "Inquilino actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el Inquilino: " + ex.Message);
                }
            }
            return View(Inquilino);
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

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                TempData["Success"] = "Inquilino eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el Inquilino: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
