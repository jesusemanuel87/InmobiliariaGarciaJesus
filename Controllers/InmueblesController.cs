using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario)]
    public class InmueblesController : Controller
    {
        private readonly IRepository<Inmueble> _inmuebleRepository;
        private readonly IRepository<Propietario> _propietarioRepository;
        private readonly IInmuebleImagenService _imagenService;
        private readonly InmuebleImagenRepository _imagenRepository;

        public InmueblesController(
            IRepository<Inmueble> inmuebleRepository, 
            IRepository<Propietario> propietarioRepository,
            IInmuebleImagenService imagenService,
            InmuebleImagenRepository imagenRepository)
        {
            _inmuebleRepository = inmuebleRepository;
            _propietarioRepository = propietarioRepository;
            _imagenService = imagenService;
            _imagenRepository = imagenRepository;
        }

        // GET: Inmuebles
        public async Task<IActionResult> Index()
        {
            try
            {
                var inmuebles = await _inmuebleRepository.GetAllAsync();
                
                // Cargar imágenes de portada para cada inmueble
                foreach (var inmueble in inmuebles)
                {
                    var imagenes = await _imagenRepository.GetByInmuebleIdAsync(inmueble.Id);
                    inmueble.Imagenes = imagenes.ToList();
                }
                
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
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: Inmuebles/UploadImages/5
        [HttpPost]
        public async Task<IActionResult> UploadImages(int id, List<IFormFile> imagenes, List<string> descripciones)
        {
            try
            {
                if (imagenes == null || !imagenes.Any())
                {
                    TempData["Error"] = "Debe seleccionar al menos una imagen";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                var inmueble = await _inmuebleRepository.GetByIdAsync(id);
                if (inmueble == null)
                {
                    TempData["Error"] = "Inmueble no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                int imagenesSubidas = 0;
                for (int i = 0; i < imagenes.Count; i++)
                {
                    if (imagenes[i] != null && imagenes[i].Length > 0)
                    {
                        var descripcion = descripciones != null && i < descripciones.Count ? descripciones[i] : null;
                        await _imagenService.GuardarImagenAsync(id, imagenes[i], descripcion);
                        imagenesSubidas++;
                    }
                }

                TempData["Success"] = $"Se subieron {imagenesSubidas} imagen(es) exitosamente";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al subir imágenes: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // POST: Inmuebles/DeleteImage/5
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id, int inmuebleId)
        {
            try
            {
                var eliminado = await _imagenService.EliminarImagenAsync(id);
                if (eliminado)
                {
                    return Json(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar la imagen" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al eliminar imagen: {ex.Message}" });
            }
        }

        // POST: Inmuebles/SetPortada/5
        [HttpPost]
        public async Task<IActionResult> SetPortada(int id, int inmuebleId)
        {
            try
            {
                var actualizado = await _imagenService.EstablecerPortadaAsync(id, inmuebleId);
                if (actualizado)
                {
                    return Json(new { success = true, message = "Imagen de portada actualizada" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar la imagen de portada" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar portada: {ex.Message}" });
            }
        }

        // GET: Inmuebles/GetImagenes/5
        [HttpGet]
        public async Task<IActionResult> GetImagenes(int id, bool readOnly = false)
        {
            try
            {
                var imagenes = await _imagenRepository.GetByInmuebleIdAsync(id);
                var viewName = readOnly ? "_ImagenesGaleriaReadOnly" : "_ImagenesGaleria";
                return PartialView(viewName, imagenes);
            }
            catch (Exception)
            {
                var viewName = readOnly ? "_ImagenesGaleriaReadOnly" : "_ImagenesGaleria";
                return PartialView(viewName, new List<InmuebleImagen>());
            }
        }
    }
}
