using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class ConfiguracionController : Controller
    {
        private readonly IRepository<Configuracion> _configuracionRepository;

        public ConfiguracionController(IRepository<Configuracion> configuracionRepository)
        {
            _configuracionRepository = configuracionRepository;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var configuraciones = await _configuracionRepository.GetAllAsync();
                return View(configuraciones);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar configuraciones: {ex.Message}";
                return View(new List<Configuracion>());
            }
        }

        public async Task<IActionResult> AdminPanel()
        {
            try
            {
                var configuraciones = await _configuracionRepository.GetAllAsync();
                
                var viewModel = new ConfiguracionAdminViewModel
                {
                    MesesMinimos = configuraciones.Where(c => c.Tipo == TipoConfiguracion.MesesMinimos).ToList(),
                    MultaTerminacionTemprana = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TEMPRANA"),
                    MultaTerminacionTardia = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TARDIA"),
                    InteresVencimiento = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_10_20"),
                    InteresVencimientoExtendido = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_20_PLUS"),
                    InteresVencimientoMensual = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_MENSUAL")
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar panel de administración: {ex.Message}";
                return View(new ConfiguracionAdminViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateMesesMinimos([FromBody] List<int> mesesHabilitados)
        {
            try
            {
                var mesesDisponibles = new[] { 6, 12, 18, 24, 30, 36 };
                
                // Obtener configuraciones existentes de meses mínimos
                var configuracionesExistentes = await _configuracionRepository.GetAllAsync();
                var mesesConfig = configuracionesExistentes.Where(c => c.Tipo == TipoConfiguracion.MesesMinimos).ToList();

                // Actualizar o crear configuraciones para cada mes
                foreach (var mes in mesesDisponibles)
                {
                    var config = mesesConfig.FirstOrDefault(c => c.Clave == $"MESES_MINIMOS_{mes}");
                    var habilitado = mesesHabilitados.Contains(mes);

                    if (config != null)
                    {
                        config.Valor = habilitado.ToString();
                        config.FechaModificacion = DateTime.Now;
                        await _configuracionRepository.UpdateAsync(config);
                    }
                    else
                    {
                        var nuevaConfig = new Configuracion
                        {
                            Clave = $"MESES_MINIMOS_{mes}",
                            Valor = habilitado.ToString(),
                            Descripcion = $"Opción de {mes} meses de alquiler mínimo",
                            Tipo = TipoConfiguracion.MesesMinimos,
                            FechaCreacion = DateTime.Now,
                            FechaModificacion = DateTime.Now
                        };
                        await _configuracionRepository.CreateAsync(nuevaConfig);
                    }
                }

                return Json(new { success = true, message = "Configuración de meses mínimos actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar configuración: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConfiguracion([FromBody] ConfiguracionUpdateModel model)
        {
            try
            {
                var config = await _configuracionRepository.GetAllAsync();
                var existingConfig = config.FirstOrDefault(c => c.Clave == model.Clave);

                if (existingConfig != null)
                {
                    existingConfig.Valor = model.Valor;
                    existingConfig.FechaModificacion = DateTime.Now;
                    await _configuracionRepository.UpdateAsync(existingConfig);
                }
                else
                {
                    var nuevaConfig = new Configuracion
                    {
                        Clave = model.Clave,
                        Valor = model.Valor,
                        Descripcion = model.Descripcion,
                        Tipo = model.Tipo,
                        FechaCreacion = DateTime.Now,
                        FechaModificacion = DateTime.Now
                    };
                    await _configuracionRepository.CreateAsync(nuevaConfig);
                }

                return Json(new { success = true, message = "Configuración actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al actualizar configuración: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var configuracion = await _configuracionRepository.GetByIdAsync(id);
                if (configuracion == null)
                {
                    TempData["Error"] = "Configuración no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(configuracion);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar configuración: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Configuracion configuracion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    configuracion.FechaCreacion = DateTime.Now;
                    configuracion.FechaModificacion = DateTime.Now;
                    await _configuracionRepository.CreateAsync(configuracion);
                    TempData["Success"] = "Configuración creada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al crear configuración: {ex.Message}";
                }
            }

            return View(configuracion);
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var configuracion = await _configuracionRepository.GetByIdAsync(id);
                if (configuracion == null)
                {
                    TempData["Error"] = "Configuración no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(configuracion);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar configuración: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Configuracion configuracion)
        {
            if (id != configuracion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    configuracion.FechaModificacion = DateTime.Now;
                    await _configuracionRepository.UpdateAsync(configuracion);
                    TempData["Success"] = "Configuración actualizada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al actualizar configuración: {ex.Message}";
                }
            }

            return View(configuracion);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var configuracion = await _configuracionRepository.GetByIdAsync(id);
                if (configuracion == null)
                {
                    TempData["Error"] = "Configuración no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                return View(configuracion);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar configuración: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _configuracionRepository.DeleteAsync(id);
                TempData["Success"] = "Configuración eliminada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar configuración: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }

}
