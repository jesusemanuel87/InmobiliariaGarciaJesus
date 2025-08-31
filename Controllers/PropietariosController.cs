using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var propietarios = await _repository.GetAllAsync();
            return View(propietarios);
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

        // GET: Propietarios/Delete/5
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

        // POST: Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
    }
}
