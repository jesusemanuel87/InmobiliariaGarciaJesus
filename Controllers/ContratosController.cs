using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.Common;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;
using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;

namespace InmobiliariaGarciaJesus.Controllers
{
    [AuthorizeMultipleRoles(RolUsuario.Empleado, RolUsuario.Administrador, RolUsuario.Propietario, RolUsuario.Inquilino)]
    public class ContratosController : Controller
    {
        private readonly IContratoService _contratoService;
        private readonly ContratoRepository _contratoRepository;
        private readonly IRepository<Inquilino> _inquilinoRepository;
        private readonly IRepository<Inmueble> _inmuebleRepository;
        private readonly IPagoService _pagoService;
        private readonly IRepository<Configuracion> _configuracionRepository;
        private readonly UsuarioRepository _usuarioRepository;

        public ContratosController(IContratoService contratoService,
                                  ContratoRepository contratoRepository,
                                  IRepository<Inquilino> inquilinoRepository,
                                  IRepository<Inmueble> inmuebleRepository,
                                  IPagoService pagoService,
                                  IRepository<Configuracion> configuracionRepository,
                                  UsuarioRepository usuarioRepository)
        {
            _contratoService = contratoService;
            _contratoRepository = contratoRepository;
            _inquilinoRepository = inquilinoRepository;
            _inmuebleRepository = inmuebleRepository;
            _pagoService = pagoService;
            _configuracionRepository = configuracionRepository;
            _usuarioRepository = usuarioRepository;
        }

        // GET: Contratos
        public async Task<IActionResult> Index(
            int page = 1,
            string[]? estado = null, 
            string? inquilino = null, 
            string? inmueble = null,
            decimal? precioMin = null, 
            decimal? precioMax = null, 
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null,
            DateTime? fechaInicioDesde = null, 
            DateTime? fechaInicioHasta = null, 
            DateTime? fechaFinDesde = null, 
            DateTime? fechaFinHasta = null)
        {
            try
            {
                // Configuración de paginación (20 items para módulo interno)
                const int pageSize = 20;
                
                // Obtener información del usuario
                var userRole = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetString("UserId");
                
                // Establecer valores por defecto
                bool isFirstLoad = !Request.Query.Any();
                if (isFirstLoad)
                {
                    estado = estado ?? new[] { "Activo", "Reservado" };
                }

                // Determinar filtros por rol
                int? inquilinoId = null;
                int? propietarioId = null;
                
                if (userRole == "Inquilino" && int.TryParse(userId, out var inquilinoUserId))
                {
                    inquilinoId = inquilinoUserId;
                }
                else if (userRole == "Propietario" && int.TryParse(userId, out var propietarioUserId))
                {
                    propietarioId = propietarioUserId;
                }

                // Convertir estados de string[] a List<EstadoContrato>
                List<EstadoContrato>? estadosEnum = null;
                if (estado != null && estado.Any() && !estado.Contains("Todos"))
                {
                    estadosEnum = estado
                        .Where(e => Enum.TryParse<EstadoContrato>(e, out _))
                        .Select(e => Enum.Parse<EstadoContrato>(e))
                        .ToList();
                }

                // ✅ OPTIMIZACIÓN: Solo traer contratos de la página actual con filtros en SQL
                var pagedResult = await _contratoRepository.GetPagedAsync(
                    page: page,
                    pageSize: pageSize,
                    inquilinoId: inquilinoId,
                    propietarioId: propietarioId,
                    estados: estadosEnum,
                    inquilinoSearch: inquilino,
                    inmuebleSearch: inmueble,
                    precioMin: precioMin,
                    precioMax: precioMax,
                    fechaInicioDesde: fechaInicioDesde,
                    fechaInicioHasta: fechaInicioHasta,
                    fechaFinDesde: fechaFinDesde,
                    fechaFinHasta: fechaFinHasta,
                    fechaCreacionDesde: fechaDesde,
                    fechaCreacionHasta: fechaHasta
                );

                // Pasar datos para los filtros
                ViewBag.PagedResult = pagedResult;
                ViewBag.Estados = estado;
                ViewBag.Inquilino = inquilino;
                ViewBag.Inmueble = inmueble;
                ViewBag.PrecioMin = precioMin;
                ViewBag.PrecioMax = precioMax;
                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;
                ViewBag.FechaInicioDesde = fechaInicioDesde;
                ViewBag.FechaInicioHasta = fechaInicioHasta;
                ViewBag.FechaFinDesde = fechaFinDesde;
                ViewBag.FechaFinHasta = fechaFinHasta;
                ViewBag.UserRole = userRole;
                ViewBag.TotalContratos = pagedResult.TotalCount;
                ViewBag.ContratosFiltrados = pagedResult.Items.Count();

                return View(pagedResult);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los contratos: " + ex.Message;
                return View(new PagedResult<Contrato>(new List<Contrato>(), 0, 1, 20));
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

        // GET: Contratos/MisContratos - Vista de contratos de inmuebles del propietario logueado
        [AuthorizeMultipleRoles(RolUsuario.Propietario)]
        public async Task<IActionResult> MisContratos()
        {
            try
            {
                // Obtener el usuario logueado usando AuthService
                var usuarioId = AuthService.GetUsuarioId(User);
                if (!usuarioId.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario?.PropietarioId == null)
                {
                    TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un propietario.";
                    return RedirectToAction("Index", "Home");
                }

                // Obtener todos los inmuebles del propietario
                var inmuebleRepo = HttpContext.RequestServices.GetService<IRepository<Inmueble>>();
                if (inmuebleRepo == null)
                {
                    TempData["Error"] = "Error al cargar datos";
                    return RedirectToAction("Index", "Home");
                }

                var todosInmuebles = await inmuebleRepo.GetAllAsync();
                var misInmuebles = todosInmuebles.Where(i => i.PropietarioId == usuario.PropietarioId.Value).ToList();
                var misInmueblesIds = misInmuebles.Select(i => i.Id).ToList();

                // Obtener contratos de esos inmuebles
                var todosContratos = await _contratoRepository.GetAllAsync();
                var misContratos = todosContratos
                    .Where(c => misInmueblesIds.Contains(c.InmuebleId))
                    .OrderByDescending(c => c.FechaInicio)
                    .ToList();

                if (!misContratos.Any())
                {
                    ViewBag.SinContratos = true;
                    ViewBag.UserRole = "Propietario";
                    return View("Index", new PagedResult<Contrato>(new List<Contrato>(), 0, 1, 20));
                }

                // Crear resultado paginado
                var resultado = new PagedResult<Contrato>(misContratos, misContratos.Count, 1, misContratos.Count);

                // Pasar datos a la vista
                ViewBag.UserRole = "Propietario";
                ViewBag.EsMisContratos = true; // Flag para que la vista sepa que es la vista del propietario
                
                return View("Index", resultado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar sus contratos: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Contratos/MiContrato - Vista del contrato del inquilino logueado
        [AuthorizeMultipleRoles(RolUsuario.Inquilino)]
        public async Task<IActionResult> MiContrato()
        {
            try
            {
                // Obtener el usuario logueado usando AuthService
                var usuarioId = AuthService.GetUsuarioId(User);
                if (!usuarioId.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId.Value);
                if (usuario?.InquilinoId == null)
                {
                    TempData["Error"] = "No tiene permisos para acceder a esta sección. Debe ser un inquilino.";
                    return RedirectToAction("Index", "Home");
                }

                // Obtener contratos del inquilino (activo y futuros)
                var todosContratos = await _contratoRepository.GetAllAsync();
                var misContratos = todosContratos
                    .Where(c => c.InquilinoId == usuario.InquilinoId.Value)
                    .OrderByDescending(c => c.FechaInicio)
                    .ToList();

                if (!misContratos.Any())
                {
                    ViewBag.SinContratos = true;
                    ViewBag.UserRole = "Inquilino";
                    return View("Index", new PagedResult<Contrato>(new List<Contrato>(), 0, 1, 20));
                }

                // Buscar contrato activo
                var fechaActual = DateTime.Now;
                var contratoActivo = misContratos.FirstOrDefault(c => 
                    c.FechaInicio <= fechaActual && 
                    c.FechaFin >= fechaActual &&
                    c.Estado != EstadoContrato.Cancelado &&
                    c.Estado != EstadoContrato.Finalizado);

                // Si no hay activo, mostrar el más reciente
                var contratoMostrar = contratoActivo ?? misContratos.First();

                // Obtener pagos del contrato
                var pagos = await _pagoService.GetPagosByContratoAsync(contratoMostrar.Id);

                // Crear resultado paginado
                var resultado = new PagedResult<Contrato>(new List<Contrato> { contratoMostrar }, 1, 1, 1);

                // Pasar datos a la vista
                ViewBag.UserRole = "Inquilino";
                ViewBag.Pagos = pagos;
                ViewBag.EsMiContrato = true; // Flag para que la vista sepa que es la vista del inquilino
                
                return View("Index", resultado);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar su contrato: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Contratos/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var inquilinos = await _inquilinoRepository.GetAllAsync(i => i.Estado == true);
                var inmuebles = await _inmuebleRepository.GetAllAsync(i => i.Estado == EstadoInmueble.Activo);
                
                // Obtener configuraciones de meses mínimos
                var configuraciones = await _configuracionRepository.GetAllAsync();
                var mesesMinimos = configuraciones.Where(c => c.Tipo == TipoConfiguracion.MesesMinimos).ToList();
                
                ViewBag.Inquilinos = inquilinos;
                ViewBag.Inmuebles = inmuebles;
                ViewBag.MesesMinimos = mesesMinimos;
                
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
                        // Obtener el ID del usuario actual para auditoría
                        var userIdClaim = User.FindFirst("UserId");
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                        {
                            contrato.CreadoPorId = userId;
                        }
                        
                        var contratoCreado = await _contratoService.CreateContratoAsync(contrato);
                        
                        // Generar plan de pagos automáticamente
                        await _pagoService.GenerarPlanPagosAsync(contratoCreado.Id);
                        
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

                var inquilinos = await _inquilinoRepository.GetAllAsync(i => i.Estado == true);
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
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
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
        [AuthorizeMultipleRoles(RolUsuario.Administrador)]
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

        // GET: Contratos/GetPagos/5
        public async Task<IActionResult> GetPagos(int id)
        {
            try
            {
                var pagos = await _pagoService.GetPagosByContratoAsync(id);
                
                if (!pagos.Any())
                {
                    return PartialView("_PagosEmpty");
                }

                return PartialView("_PagosList", pagos);
            }
            catch (Exception)
            {
                return PartialView("_PagosError");
            }
        }

        // GET: Contratos/Finalizar/5
        public async Task<IActionResult> Finalizar(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var modelo = await _contratoService.CalcularFinalizacionAsync(id.Value, DateTime.Today);
                return View(modelo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al calcular la finalización: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Finalizar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(ContratoFinalizacionViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                // Recalcular datos para mostrar la vista
                modelo = await _contratoService.CalcularFinalizacionAsync(modelo.ContratoId, modelo.FechaFinalizacion);
                return View(modelo);
            }

            // IMPORTANTE: Recalcular datos antes de finalizar para asegurar que tenemos los valores correctos
            modelo = await _contratoService.CalcularFinalizacionAsync(modelo.ContratoId, modelo.FechaFinalizacion);

            try
            {
                // Agregar logging para consola del navegador
                var logData = new {
                    action = "finalizar_contrato",
                    contratoId = modelo.ContratoId,
                    fechaFinalizacion = modelo.FechaFinalizacion,
                    multaCalculada = modelo.MultaCalculada,
                    importeAdeudado = modelo.ImporteAdeudado,
                    esFinalizacionTemprana = modelo.EsFinalizacionTemprana
                };
                
                // Obtener el ID del usuario actual para auditoría
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Obtener el contrato y actualizar información de auditoría
                    var contrato = await _contratoService.GetByIdAsync(modelo.ContratoId);
                    if (contrato != null)
                    {
                        contrato.TerminadoPorId = userId;
                        contrato.FechaTerminacion = DateTime.Now;
                    }
                }
                
                await _contratoService.FinalizarContratoAsync(modelo);
                
                // Retornar JSON para mostrar modal de confirmación de pago
                var multaSoloTerminacion = modelo.MultaCalculada.HasValue ? modelo.MultaCalculada.Value - modelo.ImporteAdeudado : 0;
                return Json(new { 
                    success = true, 
                    contratoId = modelo.ContratoId,
                    multaTerminacion = multaSoloTerminacion,
                    multaTotal = modelo.MultaCalculada,
                    importeAdeudado = modelo.ImporteAdeudado,
                    message = "Contrato finalizado exitosamente. ¿Desea procesar el pago ahora?",
                    logData = logData
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al finalizar el contrato: " + ex.Message });
            }
        }

        // POST: Contratos/ProcesarPagoFinalizacion/5
        [HttpPost]
        public async Task<IActionResult> ProcesarPagoFinalizacion(int contratoId, bool procesarPago)
        {
            try
            {
                if (procesarPago)
                {
                    await _contratoService.ProcesarPagoFinalizacionAsync(contratoId);
                    TempData["Success"] = "Contrato finalizado y pago procesado exitosamente.";
                }
                else
                {
                    TempData["Success"] = "Contrato finalizado. El pago queda pendiente.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al procesar el pago: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Contratos/Cancelar/5
        public async Task<IActionResult> Cancelar(int? id)
        {
            if (id == null) return NotFound();

            try
            {
                var contrato = await _contratoService.GetByIdAsync(id.Value);
                if (contrato == null) return NotFound();

                return View(contrato);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el contrato: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Contratos/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                ModelState.AddModelError("MotivoCancelacion", "El motivo de cancelación es obligatorio");
                var contrato = await _contratoService.GetByIdAsync(id);
                return View(contrato);
            }

            try
            {
                // Obtener el ID del usuario actual para auditoría
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Obtener el contrato y actualizar información de auditoría
                    var contrato = await _contratoService.GetByIdAsync(id);
                    if (contrato != null)
                    {
                        contrato.TerminadoPorId = userId;
                        contrato.FechaTerminacion = DateTime.Now;
                    }
                }
                
                await _contratoService.CancelarContratoAsync(id, motivo);
                TempData["Success"] = "Contrato cancelado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cancelar el contrato: " + ex.Message;
                var contrato = await _contratoService.GetByIdAsync(id);
                return View(contrato);
            }
        }

        // GET: Contratos/AuditoriaModal/5
        [HttpGet]
        [AuthorizeRole(RolUsuario.Administrador)]
        public async Task<IActionResult> AuditoriaModal(int id)
        {
            try
            {
                var contrato = await _contratoService.GetByIdAsync(id);
                if (contrato == null)
                {
                    return NotFound();
                }

                // Obtener información de usuarios para auditoría
                Usuario? creadoPor = null;
                Usuario? terminadoPor = null;

                if (contrato.CreadoPorId.HasValue)
                {
                    creadoPor = await _usuarioRepository.GetByIdAsync(contrato.CreadoPorId.Value);
                }

                if (contrato.TerminadoPorId.HasValue)
                {
                    terminadoPor = await _usuarioRepository.GetByIdAsync(contrato.TerminadoPorId.Value);
                }

                // Crear ViewModel de auditoría
                var auditoriaViewModel = new ContratoAuditoriaViewModel
                {
                    TipoEntidad = "Contrato",
                    EntidadId = contrato.Id,
                    NumeroContrato = contrato.Id,
                    NombreInquilino = contrato.Inquilino != null ? $"{contrato.Inquilino.Nombre} {contrato.Inquilino.Apellido}" : "No especificado",
                    DireccionInmueble = contrato.Inmueble?.Direccion ?? "No especificado",
                    FechaCreacion = contrato.FechaCreacion,
                    CreadoPor = creadoPor?.NombreUsuario ?? "Sistema",
                    UsuarioCreador = creadoPor?.NombreUsuario,
                    FechaTerminacion = contrato.FechaTerminacion,
                    TerminadoPor = terminadoPor?.NombreUsuario,
                    UsuarioTerminador = terminadoPor?.NombreUsuario,
                    EstadoActual = contrato.Estado.ToString(),
                    AccionRealizada = contrato.FechaTerminacion.HasValue ? "Terminación del Contrato" : null,
                    FechaModificacion = contrato.FechaTerminacion,
                    ModificadoPor = terminadoPor?.NombreUsuario,
                    UsuarioModificador = terminadoPor?.NombreUsuario,
                    Observaciones = $"Contrato por ${contrato.Precio:N0} desde {contrato.FechaInicio:dd/MM/yyyy} hasta {contrato.FechaFin:dd/MM/yyyy}"
                };

                return PartialView("_ContratoAuditoriaModal", auditoriaViewModel);
            }
            catch (Exception ex)
            {
                // Log error
                return BadRequest("Error al cargar información de auditoría");
            }
        }
    }
}
