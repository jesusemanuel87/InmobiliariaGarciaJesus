using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly IRepository<Inmueble> _inmuebleRepository;
        private readonly IRepository<Propietario> _propietarioRepository;

        public InmueblesController(IRepository<Inmueble> inmuebleRepository, IRepository<Propietario> propietarioRepository)
        {
            _inmuebleRepository = inmuebleRepository;
            _propietarioRepository = propietarioRepository;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index()
        {
            try
            {
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                return View(inmuebles);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los inmuebles: " + ex.Message;
                return View(new List<Inmueble>());
            }
        }

        // GET: Inmuebles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var propietarios = await _propietarioRepository.GetAllAsync();
                ViewBag.Propietarios = propietarios;
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los propietarios: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,Tipo,Precio,Uso")] Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    inmueble.FechaCreacion = DateTime.Now;
                    inmueble.Estado = EstadoInmueble.Activo;
                    
                    await _inmuebleRepository.CreateAsync(inmueble);
                    TempData["Success"] = "Inmueble creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al crear el inmueble: " + ex.Message;
                }
            }

            var propietarios = await _propietarioRepository.GetAllAsync();
            ViewBag.Propietarios = propietarios;
            return View(inmueble);
        }

        // GET: Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                var propietarios = await _propietarioRepository.GetAllAsync();
                ViewBag.Propietarios = propietarios;
                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Direccion,Ambientes,Superficie,Latitud,Longitud,PropietarioId,Tipo,Precio,Uso,FechaCreacion,Estado")] Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _inmuebleRepository.UpdateAsync(inmueble);
                    TempData["Success"] = "Inmueble actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error al actualizar el inmueble: " + ex.Message;
                }
            }

            var propietarios = await _propietarioRepository.GetAllAsync();
            ViewBag.Propietarios = propietarios;
            return View(inmueble);
        }

        // GET: Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var inmueble = await _inmuebleRepository.GetByIdAsync(id.Value);
                if (inmueble == null)
                {
                    return NotFound();
                }

                return View(inmueble);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _inmuebleRepository.DeleteAsync(id);
                TempData["Success"] = "Inmueble eliminado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el inmueble: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // API endpoint para obtener inmuebles con precios
        [HttpGet]
        public async Task<IActionResult> GetInmueblesConPrecios()
        {
            try
            {
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                var inmueblesConPrecios = inmuebles
                    .Where(i => i.Precio.HasValue && i.Estado == EstadoInmueble.Activo)
                    .Select(i => new { 
                        id = i.Id, 
                        direccion = i.Direccion,
                        precio = i.Precio.Value 
                    })
                    .ToList();
                
                return Json(inmueblesConPrecios);
            }
            catch (Exception)
            {
                return Json(new List<object>());
            }
        }
    }
}
