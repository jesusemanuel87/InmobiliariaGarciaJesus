using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly InmobiliariaContext _context;

        public PropietariosController(InmobiliariaContext context)
        {
            _context = context;
        }

        // GET: Propietarios
        public async Task<IActionResult> Index()
        {
            var propietarios = await _context.Propietarios
                .Where(p => p.Estado == true)
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToListAsync();
            return View(propietarios);
        }

        // GET: Propietarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.Propietarios
                .Include(p => p.Inmuebles)
                .FirstOrDefaultAsync(m => m.Id == id);
            
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
        public async Task<IActionResult> Create([Bind("DNI,Nombre,Apellido,Telefono,Email,Direccion")] Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe un propietario con el mismo DNI
                    var existeDNI = await _context.Propietarios.AnyAsync(p => p.DNI == propietario.DNI);
                    if (existeDNI)
                    {
                        ModelState.AddModelError("DNI", "Ya existe un propietario con este DNI.");
                        return View(propietario);
                    }

                    // Verificar si ya existe un propietario con el mismo Email
                    var existeEmail = await _context.Propietarios.AnyAsync(p => p.Email == propietario.Email);
                    if (existeEmail)
                    {
                        ModelState.AddModelError("Email", "Ya existe un propietario con este Email.");
                        return View(propietario);
                    }

                    propietario.FechaCreacion = DateTime.Now;
                    propietario.Estado = true;
                    
                    _context.Add(propietario);
                    await _context.SaveChangesAsync();
                    
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

            var propietario = await _context.Propietarios.FindAsync(id);
            if (propietario == null)
            {
                return NotFound();
            }
            return View(propietario);
        }

        // POST: Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DNI,Nombre,Apellido,Telefono,Email,Direccion,FechaCreacion,Estado")] Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe otro propietario con el mismo DNI
                    var existeDNI = await _context.Propietarios.AnyAsync(p => p.DNI == propietario.DNI && p.Id != propietario.Id);
                    if (existeDNI)
                    {
                        ModelState.AddModelError("DNI", "Ya existe otro propietario con este DNI.");
                        return View(propietario);
                    }

                    // Verificar si ya existe otro propietario con el mismo Email
                    var existeEmail = await _context.Propietarios.AnyAsync(p => p.Email == propietario.Email && p.Id != propietario.Id);
                    if (existeEmail)
                    {
                        ModelState.AddModelError("Email", "Ya existe otro propietario con este Email.");
                        return View(propietario);
                    }

                    _context.Update(propietario);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Propietario actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropietarioExists(propietario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
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

            var propietario = await _context.Propietarios
                .Include(p => p.Inmuebles)
                .FirstOrDefaultAsync(m => m.Id == id);
            
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
                var propietario = await _context.Propietarios
                    .Include(p => p.Inmuebles)
                    .FirstOrDefaultAsync(p => p.Id == id);
                
                if (propietario != null)
                {
                    // Verificar si tiene inmuebles asociados
                    if (propietario.Inmuebles != null && propietario.Inmuebles.Any())
                    {
                        TempData["Error"] = "No se puede eliminar el propietario porque tiene inmuebles asociados.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Eliminación lógica
                    propietario.Estado = false;
                    _context.Update(propietario);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Propietario eliminado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el propietario: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PropietarioExists(int id)
        {
            return _context.Propietarios.Any(e => e.Id == id);
        }
    }
}
