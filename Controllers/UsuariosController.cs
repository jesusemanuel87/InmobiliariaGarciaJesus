using Microsoft.AspNetCore.Mvc;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Attributes;
using InmobiliariaGarciaJesus.Models.Common;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class UsuarioIdRequest
    {
        public int Id { get; set; }
    }

    [AuthorizeMultipleRoles(RolUsuario.Administrador)]
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly IRepository<Propietario> _propietarioRepository;
        private readonly IRepository<Inquilino> _inquilinoRepository;
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            UsuarioRepository usuarioRepository,
            IRepository<Propietario> propietarioRepository,
            IRepository<Inquilino> inquilinoRepository,
            UsuarioService usuarioService,
            ILogger<UsuariosController> logger)
        {
            _usuarioRepository = usuarioRepository;
            _propietarioRepository = propietarioRepository;
            _inquilinoRepository = inquilinoRepository;
            _usuarioService = usuarioService;
            _logger = logger;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? tipo = null, string? estado = null, string? buscar = null)
        {
            try
            {
                var todosUsuarios = await _usuarioRepository.GetAllAsync();
                
                // Filtrar por tipo (rol)
                if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
                {
                    if (Enum.TryParse<RolUsuario>(tipo, out var rolFiltro))
                    {
                        todosUsuarios = todosUsuarios.Where(u => u.Rol == rolFiltro);
                    }
                }

                // Filtrar por estado (solo si se especifica explícitamente)
                if (!string.IsNullOrEmpty(estado) && estado != "Todos")
                {
                    bool estadoBool = estado == "Activo";
                    todosUsuarios = todosUsuarios.Where(u => u.Estado == estadoBool);
                }

                // Buscar por nombre, email, username
                if (!string.IsNullOrEmpty(buscar))
                {
                    var buscarLower = buscar.ToLower();
                    todosUsuarios = todosUsuarios.Where(u =>
                        u.NombreUsuario.ToLower().Contains(buscarLower) ||
                        u.Email.ToLower().Contains(buscarLower) ||
                        (u.Propietario != null && (u.Propietario.Nombre.ToLower().Contains(buscarLower) || u.Propietario.Apellido.ToLower().Contains(buscarLower))) ||
                        (u.Inquilino != null && (u.Inquilino.Nombre.ToLower().Contains(buscarLower) || u.Inquilino.Apellido.ToLower().Contains(buscarLower)))
                    );
                }

                var usuarios = todosUsuarios.OrderByDescending(u => u.FechaCreacion).ToList();

                ViewBag.Tipo = tipo;
                ViewBag.Estado = estado;
                ViewBag.Buscar = buscar;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios");
                TempData["Error"] = "Error al cargar los usuarios";
                return View(new List<Usuario>());
            }
        }

        // POST: Usuarios/ActivarUsuario
        [HttpPost]
        public async Task<IActionResult> ActivarUsuario([FromBody] UsuarioIdRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(request.Id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Activar usuario
                usuario.Estado = true;
                var successUsuario = await _usuarioRepository.UpdateAsync(usuario);

                // Activar persona asociada (Propietario o Inquilino)
                bool successPersona = true;
                if (usuario.PropietarioId.HasValue)
                {
                    var propietario = await _propietarioRepository.GetByIdAsync(usuario.PropietarioId.Value);
                    if (propietario != null)
                    {
                        propietario.Estado = true;
                        successPersona = await _propietarioRepository.UpdateAsync(propietario);
                    }
                }
                else if (usuario.InquilinoId.HasValue)
                {
                    var inquilino = await _inquilinoRepository.GetByIdAsync(usuario.InquilinoId.Value);
                    if (inquilino != null)
                    {
                        inquilino.Estado = true;
                        successPersona = await _inquilinoRepository.UpdateAsync(inquilino);
                    }
                }

                if (successUsuario && successPersona)
                {
                    _logger.LogInformation("Usuario activado: {UsuarioId} por Admin", request.Id);
                    return Json(new { success = true, message = "Usuario activado exitosamente" });
                }

                return Json(new { success = false, message = "Error al activar el usuario" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar usuario: {UsuarioId}", request.Id);
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // POST: Usuarios/DesactivarUsuario
        [HttpPost]
        public async Task<IActionResult> DesactivarUsuario([FromBody] UsuarioIdRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(request.Id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Desactivar usuario
                usuario.Estado = false;
                var successUsuario = await _usuarioRepository.UpdateAsync(usuario);

                // Desactivar persona asociada (Propietario o Inquilino)
                bool successPersona = true;
                if (usuario.PropietarioId.HasValue)
                {
                    var propietario = await _propietarioRepository.GetByIdAsync(usuario.PropietarioId.Value);
                    if (propietario != null)
                    {
                        propietario.Estado = false;
                        successPersona = await _propietarioRepository.UpdateAsync(propietario);
                    }
                }
                else if (usuario.InquilinoId.HasValue)
                {
                    var inquilino = await _inquilinoRepository.GetByIdAsync(usuario.InquilinoId.Value);
                    if (inquilino != null)
                    {
                        inquilino.Estado = false;
                        successPersona = await _inquilinoRepository.UpdateAsync(inquilino);
                    }
                }

                if (successUsuario && successPersona)
                {
                    _logger.LogInformation("Usuario desactivado: {UsuarioId} por Admin", request.Id);
                    return Json(new { success = true, message = "Usuario desactivado exitosamente" });
                }

                return Json(new { success = false, message = "Error al desactivar el usuario" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario: {UsuarioId}", request.Id);
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        // POST: Usuarios/CrearUsuarioParaPersona
        [HttpPost]
        public async Task<IActionResult> CrearUsuarioParaPersona(int personaId, string tipo)
        {
            try
            {
                _logger.LogInformation("CrearUsuarioParaPersona: PersonaId={PersonaId}, Tipo={Tipo}", personaId, tipo);

                if (tipo != "Propietario" && tipo != "Inquilino")
                {
                    return Json(new { success = false, message = "Tipo de usuario inválido" });
                }

                var tipoUsuario = tipo == "Propietario" ? RolUsuario.Propietario : RolUsuario.Inquilino;

                // Verificar que la persona exista y no tenga usuario
                Usuario? usuario = null;
                string nombre = "";
                string apellido = "";
                string dni = "";
                string telefono = "";
                string email = "";

                if (tipoUsuario == RolUsuario.Propietario)
                {
                    var propietario = await _propietarioRepository.GetByIdAsync(personaId);
                    if (propietario == null)
                    {
                        return Json(new { success = false, message = "Propietario no encontrado" });
                    }

                    // Verificar que no tenga usuario
                    var usuarios = await _usuarioRepository.GetUsuariosByPersonaAsync(personaId, RolUsuario.Propietario);
                    if (usuarios.Any())
                    {
                        return Json(new { success = false, message = "Este propietario ya tiene un usuario asociado" });
                    }

                    nombre = propietario.Nombre;
                    apellido = propietario.Apellido;
                    dni = propietario.Dni;
                    telefono = propietario.Telefono;
                    email = propietario.Email;
                }
                else
                {
                    var inquilino = await _inquilinoRepository.GetByIdAsync(personaId);
                    if (inquilino == null)
                    {
                        return Json(new { success = false, message = "Inquilino no encontrado" });
                    }

                    // Verificar que no tenga usuario
                    var usuarios = await _usuarioRepository.GetUsuariosByPersonaAsync(personaId, RolUsuario.Inquilino);
                    if (usuarios.Any())
                    {
                        return Json(new { success = false, message = "Este inquilino ya tiene un usuario asociado" });
                    }

                    nombre = inquilino.Nombre;
                    apellido = inquilino.Apellido;
                    dni = inquilino.Dni;
                    telefono = inquilino.Telefono;
                    email = inquilino.Email;
                }

                // Generar username único
                var baseUsername = $"{nombre.ToLower()}.{apellido.ToLower()}".Replace(" ", "");
                var username = baseUsername;
                int contador = 1;
                while (await _usuarioService.GetUsuarioByUsernameAsync(username) != null)
                {
                    username = $"{baseUsername}{contador}";
                    contador++;
                }

                // Crear usuario con contraseña = DNI
                usuario = new Usuario
                {
                    NombreUsuario = username,
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dni), // Contraseña = DNI
                    Rol = tipoUsuario,
                    Estado = false, // Inactivo por defecto, admin debe activar
                    RequiereCambioClave = true, // Forzar cambio de contraseña en primer login
                    FechaCreacion = DateTime.Now,
                    UltimoAcceso = DateTime.Now
                };

                if (tipoUsuario == RolUsuario.Propietario)
                {
                    usuario.PropietarioId = personaId;
                }
                else
                {
                    usuario.InquilinoId = personaId;
                }

                var usuarioId = await _usuarioRepository.CreateAsync(usuario);

                if (usuarioId > 0)
                {
                    _logger.LogInformation("Usuario creado: {UsuarioId} para {Tipo} {PersonaId}", usuarioId, tipo, personaId);
                    return Json(new 
                    { 
                        success = true, 
                        message = $"Usuario creado exitosamente. Username: {username}, Contraseña temporal: {dni} (DNI)",
                        username = username
                    });
                }

                return Json(new { success = false, message = "Error al crear el usuario" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario para persona: {PersonaId}", personaId);
                return Json(new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        // POST: Usuarios/RestablecerContrasena
        [HttpPost]
        public async Task<IActionResult> RestablecerContrasena([FromBody] UsuarioIdRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(request.Id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Obtener DNI
                string dni = "";
                if (usuario.PropietarioId.HasValue)
                {
                    var propietario = await _propietarioRepository.GetByIdAsync(usuario.PropietarioId.Value);
                    dni = propietario?.Dni ?? "";
                }
                else if (usuario.InquilinoId.HasValue)
                {
                    var inquilino = await _inquilinoRepository.GetByIdAsync(usuario.InquilinoId.Value);
                    dni = inquilino?.Dni ?? "";
                }

                if (string.IsNullOrEmpty(dni))
                {
                    return Json(new { success = false, message = "No se pudo obtener el DNI del usuario" });
                }

                // Restablecer contraseña = DNI
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dni);
                usuario.RequiereCambioClave = true;
                var success = await _usuarioRepository.UpdateAsync(usuario);

                if (success)
                {
                    _logger.LogInformation("Contraseña restablecida para usuario: {UsuarioId} por Admin", request.Id);
                    return Json(new { success = true, message = $"Contraseña restablecida a: {dni} (DNI)" });
                }

                return Json(new { success = false, message = "Error al restablecer la contraseña" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña: {UsuarioId}", request.Id);
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }
    }
}
