using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly InmobiliariaContext _context;

        public InquilinosController(InmobiliariaContext context)
        {
            _context = context;
        }

        // GET: Inquilinos
        public async Task<IActionResult> Index()
        {
            var inquilinos = await _context.Inquilinos
                .Where(i => i.Estado == true)
                .OrderBy(i => i.Apellido)
                .ThenBy(i => i.Nombre)
                .ToListAsync();
            return View(inquilinos);
        }

        // GET: Inquilinos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .Include(i => i.Contratos)
                .ThenInclude(c => c.Inmueble)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DNI,Nombre,Apellido,Telefono,Email,Direccion")] Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe un inquilino con el mismo DNI
                    var existeDNI = await _context.Inquilinos.AnyAsync(i => i.DNI == inquilino.DNI);
                    if (existeDNI)
                    {
                        ModelState.AddModelError("DNI", "Ya existe un inquilino con este DNI.");
                        return View(inquilino);
                    }

                    // Verificar si ya existe un inquilino con el mismo Email
                    var existeEmail = await _context.Inquilinos.AnyAsync(i => i.Email == inquilino.Email);
                    if (existeEmail)
                    {
                        ModelState.AddModelError("Email", "Ya existe un inquilino con este Email.");
                        return View(inquilino);
                    }

                    inquilino.FechaCreacion = DateTime.Now;
                    inquilino.Estado = true;
                    
                    _context.Add(inquilino);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Inquilino creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el inquilino: " + ex.Message);
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos.FindAsync(id);
            if (inquilino == null)
            {
                return NotFound();
            }
            return View(inquilino);
        }

        // POST: Inquilinos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DNI,Nombre,Apellido,Telefono,Email,Direccion,FechaCreacion,Estado")] Inquilino inquilino)
        {
            if (id != inquilino.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si ya existe otro inquilino con el mismo DNI
                    var existeDNI = await _context.Inquilinos.AnyAsync(i => i.DNI == inquilino.DNI && i.Id != inquilino.Id);
                    if (existeDNI)
                    {
                        ModelState.AddModelError("DNI", "Ya existe otro inquilino con este DNI.");
                        return View(inquilino);
                    }

                    // Verificar si ya existe otro inquilino con el mismo Email
                    var existeEmail = await _context.Inquilinos.AnyAsync(i => i.Email == inquilino.Email && i.Id != inquilino.Id);
                    if (existeEmail)
                    {
                        ModelState.AddModelError("Email", "Ya existe otro inquilino con este Email.");
                        return View(inquilino);
                    }

                    _context.Update(inquilino);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Inquilino actualizado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InquilinoExists(inquilino.Id))
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
                    ModelState.AddModelError("", "Error al actualizar el inquilino: " + ex.Message);
                }
            }
            return View(inquilino);
        }

        // GET: Inquilinos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquilino = await _context.Inquilinos
                .Include(i => i.Contratos)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (inquilino == null)
            {
                return NotFound();
            }

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var inquilino = await _context.Inquilinos
                    .Include(i => i.Contratos)
                    .FirstOrDefaultAsync(i => i.Id == id);
                
                if (inquilino != null)
                {
                    // Verificar si tiene contratos activos
                    if (inquilino.Contratos != null && inquilino.Contratos.Any(c => c.Estado == EstadoContrato.Activo))
                    {
                        TempData["Error"] = "No se puede eliminar el inquilino porque tiene contratos activos.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Eliminación lógica
                    inquilino.Estado = false;
                    _context.Update(inquilino);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Inquilino eliminado exitosamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el inquilino: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InquilinoExists(int id)
        {
            return _context.Inquilinos.Any(e => e.Id == id);
        }
    }
}
