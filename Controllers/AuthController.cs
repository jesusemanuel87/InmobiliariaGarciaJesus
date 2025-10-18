using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaGarciaJesus.Attributes;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Services;
using InmobiliariaGarciaJesus.Repositories;
using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;
using System.Security.Claims;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly AuthService _authenticationService;
        private readonly ProfilePhotoService _profilePhotoService;
        private readonly ILogger<AuthController> _logger;
        private readonly UsuarioRepository _usuarioRepository;

        public AuthController(
            UsuarioService usuarioService,
            AuthService authenticationService,
            ProfilePhotoService profilePhotoService,
            ILogger<AuthController> logger,
            UsuarioRepository usuarioRepository)
        {
            _usuarioService = usuarioService;
            _authenticationService = authenticationService;
            _profilePhotoService = profilePhotoService;
            _logger = logger;
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? message = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // Mostrar mensaje si la sesión expiró
            if (message == "session_expired")
            {
                TempData["ErrorMessage"] = "Su sesión ha expirado debido al reinicio del servidor. Por favor, inicie sesión nuevamente.";
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, usuario) = await _usuarioService.AuthenticateAsync(model.Email, model.Password);

            if (!success || usuario == null)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            var claimsPrincipal = await _authenticationService.CreateClaimsPrincipalAsync(usuario);
            if (claimsPrincipal == null)
            {
                ModelState.AddModelError(string.Empty, "Error interno del sistema");
                return View(model);
            }

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // Establecer información de sesión
            HttpContext.Session.SetString("UserId", usuario.Id.ToString());
            HttpContext.Session.SetString("UserEmail", usuario.Email);
            HttpContext.Session.SetString("UserRole", usuario.Rol.ToString());
            HttpContext.Session.SetString("UserName", usuario.NombreCompleto);

            _logger.LogInformation("Usuario logueado exitosamente: {Email}", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirigir según el rol del usuario
            return usuario.Rol switch
            {
                RolUsuario.Administrador or RolUsuario.Empleado => RedirectToAction("Dashboard", "Home"),
                RolUsuario.Propietario => RedirectToAction("MisInmuebles", "Inmuebles"),
                RolUsuario.Inquilino => RedirectToAction("MiContrato", "Contratos"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? existingPassword, bool isMultiRole = false)
        {
            // Si es modo multi-rol, eliminar validaciones de contraseña
            if (isMultiRole && !string.IsNullOrEmpty(existingPassword))
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Detectar si es modo multi-rol
            if (isMultiRole && !string.IsNullOrEmpty(existingPassword))
            {
                // Registro multi-rol: copiar datos de cuenta existente
                var (success, message, usuarioId) = await _usuarioService.RegisterMultiRoleUsuarioAsync(model, existingPassword);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(model);
                }

                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Login));
            }
            else
            {
                // Registro normal
                var (success, message, usuarioId) = await _usuarioService.RegisterUsuarioAsync(model);

                if (!success)
                {
                    ModelState.AddModelError(string.Empty, message);
                    return View(model);
                }

                TempData["SuccessMessage"] = message;
                return RedirectToAction(nameof(Login));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Limpiar sesión
            HttpContext.Session.Clear();
            
            _logger.LogInformation("Usuario deslogueado");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AuthorizeMultipleRoles(RolUsuario.Propietario, RolUsuario.Inquilino, RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> Profile()
        {
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId.Value);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var rolesDisponibles = await _usuarioService.GetRolesDisponiblesAsync(usuario);

            var profileViewModel = new ProfileViewModel
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Email = usuario.Email,
                Rol = usuario.Rol,
                FotoPerfil = usuario.FotoPerfil,
                FechaCreacion = usuario.FechaCreacion,
                UltimoAcceso = usuario.UltimoAcceso,
                RolesDisponibles = rolesDisponibles
            };

            // Cargar información específica según el rol
            switch (usuario.Rol)
            {
                case RolUsuario.Propietario when usuario.Propietario != null:
                    profileViewModel.Nombre = usuario.Propietario.Nombre;
                    profileViewModel.Apellido = usuario.Propietario.Apellido;
                    profileViewModel.Dni = usuario.Propietario.Dni;
                    profileViewModel.Telefono = usuario.Propietario.Telefono;
                    break;
                case RolUsuario.Inquilino when usuario.Inquilino != null:
                    profileViewModel.Nombre = usuario.Inquilino.Nombre;
                    profileViewModel.Apellido = usuario.Inquilino.Apellido;
                    profileViewModel.Dni = usuario.Inquilino.Dni;
                    profileViewModel.Telefono = usuario.Inquilino.Telefono;
                    break;
                case RolUsuario.Empleado or RolUsuario.Administrador when usuario.Empleado != null:
                    profileViewModel.Nombre = usuario.Empleado.Nombre;
                    profileViewModel.Apellido = usuario.Empleado.Apellido;
                    profileViewModel.Dni = usuario.Empleado.Dni;
                    profileViewModel.Telefono = usuario.Empleado.Telefono;
                    profileViewModel.RolEmpleado = usuario.Empleado.Rol;
                    profileViewModel.FechaIngreso = usuario.Empleado.FechaIngreso;
                    break;
            }

            return View(profileViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Propietario, RolUsuario.Inquilino, RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Por favor, complete todos los campos correctamente.";
                return RedirectToAction(nameof(Profile));
            }

            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _usuarioService.ChangePasswordAsync(usuarioId.Value, model.CurrentPassword, model.NewPassword);
            bool success = result.Item1;
            string message = result.Item2;

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Propietario, RolUsuario.Inquilino, RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> SwitchRole(SwitchRoleViewModel model)
        {
            try
            {
                _logger.LogInformation("SwitchRole: UsuarioId={UsuarioId}, NuevoRol={NuevoRol}", model.UsuarioId, model.NuevoRol);
                
                var usuarioId = AuthService.GetUsuarioId(User);
                if (!usuarioId.HasValue || usuarioId.Value != model.UsuarioId)
                {
                    _logger.LogWarning("SwitchRole: Usuario no autorizado. UsuarioId del claim={ClaimId}, UsuarioId del modelo={ModelId}", 
                        usuarioId, model.UsuarioId);
                    TempData["ErrorMessage"] = "No tiene permisos para cambiar el rol de otro usuario";
                    return RedirectToAction(nameof(Login));
                }

                // Buscar el usuario con el nuevo rol (mismo email, diferente rol)
                var usuarioActual = await _usuarioService.GetUsuarioByIdAsync(model.UsuarioId);
                if (usuarioActual == null)
                {
                    _logger.LogError("SwitchRole: Usuario no encontrado. UsuarioId={UsuarioId}", model.UsuarioId);
                    TempData["ErrorMessage"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Login));
                }

                // Buscar el usuario con el mismo email y el nuevo rol
                var todosUsuarios = await _usuarioRepository.GetAllAsync();
                var usuarioNuevoRol = todosUsuarios.FirstOrDefault(u => 
                    u.Email.Equals(usuarioActual.Email, StringComparison.OrdinalIgnoreCase) && 
                    u.Rol == model.NuevoRol &&
                    u.Estado);

                if (usuarioNuevoRol == null)
                {
                    _logger.LogError("SwitchRole: No se encontró usuario con email={Email} y rol={Rol}", 
                        usuarioActual.Email, model.NuevoRol);
                    TempData["ErrorMessage"] = "No tiene permisos para cambiar a este rol";
                    return RedirectToAction(nameof(Profile));
                }

                _logger.LogInformation("SwitchRole: Cambiando de usuario {IdAnterior} a usuario {IdNuevo}", 
                    model.UsuarioId, usuarioNuevoRol.Id);

                // Cerrar sesión actual
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Crear claims del nuevo usuario
                var claimsPrincipal = await _authenticationService.CreateClaimsPrincipalAsync(usuarioNuevoRol);
                if (claimsPrincipal != null)
                {
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    
                    // Actualizar último acceso
                    usuarioNuevoRol.UltimoAcceso = DateTime.Now;
                    await _usuarioRepository.UpdateAsync(usuarioNuevoRol);
                }

                TempData["SuccessMessage"] = $"Rol cambiado exitosamente a {model.NuevoRol}";
                
                // Redirigir según el nuevo rol
                return model.NuevoRol switch
                {
                    RolUsuario.Administrador or RolUsuario.Empleado => RedirectToAction("Dashboard", "Home"),
                    RolUsuario.Propietario => RedirectToAction("MisInmuebles", "Inmuebles"),
                    RolUsuario.Inquilino => RedirectToAction("MiContrato", "Contratos"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SwitchRole: Error inesperado. UsuarioId={UsuarioId}, NuevoRol={NuevoRol}", 
                    model.UsuarioId, model.NuevoRol);
                TempData["ErrorMessage"] = "Error al cambiar de rol: " + ex.Message;
                return RedirectToAction(nameof(Profile));
            }
        }

        // Método para verificar si un email está disponible (AJAX)
        // Detecta si email existe con rol diferente para activar modo multi-rol
        [HttpPost]
        public async Task<IActionResult> CheckEmailAvailability(string email, int? rol)
        {
            if (!rol.HasValue)
            {
                // Si no se especifica rol, validar globalmente (comportamiento anterior)
                var exists = await _usuarioService.GetUsuarioByEmailAsync(email) != null;
                return Json(new { available = !exists });
            }
            
            var tipoUsuario = (RolUsuario)rol.Value;
            
            // Primero verificar si email existe con el MISMO rol (no permitido)
            var existsWithSameRole = await _usuarioRepository.EmailExistsWithRoleAsync(email, tipoUsuario);
            if (existsWithSameRole)
            {
                // Email + mismo rol → No disponible (error normal)
                return Json(new { available = false, sameRole = true });
            }
            
            // Verificar si email existe con DIFERENTE rol (activar modo multi-rol)
            var existingUser = await _usuarioService.GetUsuarioByEmailAsync(email);
            if (existingUser != null)
            {
                // Email existe con diferente rol → No disponible, pero activar validación multi-rol
                return Json(new { available = false, existingEmail = true });
            }
            
            // Email no existe → Disponible para registro normal
            return Json(new { available = true });
        }

        // Método para verificar si un username está disponible (AJAX)
        [HttpPost]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var exists = await _usuarioService.GetUsuarioByUsernameAsync(username) != null;
            return Json(new { available = !exists });
        }

        // Método para validar email existente y obtener datos para multi-rol (AJAX)
        [HttpPost]
        public async Task<IActionResult> ValidateExistingEmailForMultiRole(string email, int rol, string password)
        {
            var tipoUsuario = (RolUsuario)rol;
            var (exists, usuario, errorMessage) = await _usuarioService.ValidateExistingEmailForMultiRoleAsync(email, tipoUsuario, password);

            if (!exists && errorMessage != null)
            {
                return Json(new { 
                    success = false, 
                    error = errorMessage 
                });
            }

            if (!exists)
            {
                // Email no existe, continuar con registro normal
                return Json(new { 
                    success = false, 
                    emailNotFound = true 
                });
            }

            // Usuario validado, devolver datos para autocompletar
            string nombre = "", apellido = "", dni = "", telefono = "";
            
            if (usuario == null)
            {
                return Json(new { success = false, error = "Error al obtener datos del usuario" });
            }
            
            if (usuario.Propietario != null)
            {
                nombre = usuario.Propietario.Nombre;
                apellido = usuario.Propietario.Apellido;
                dni = usuario.Propietario.Dni;
                telefono = usuario.Propietario.Telefono ?? "";
            }
            else if (usuario.Inquilino != null)
            {
                nombre = usuario.Inquilino.Nombre;
                apellido = usuario.Inquilino.Apellido;
                dni = usuario.Inquilino.Dni;
                telefono = usuario.Inquilino.Telefono ?? "";
            }
            else if (usuario.Empleado != null)
            {
                nombre = usuario.Empleado.Nombre;
                apellido = usuario.Empleado.Apellido;
                dni = usuario.Empleado.Dni;
                telefono = usuario.Empleado.Telefono;
            }

            return Json(new { 
                success = true,
                data = new {
                    nombre,
                    apellido,
                    dni,
                    telefono,
                    existingUsername = usuario.NombreUsuario,
                    suggestedUsername = $"{usuario.NombreUsuario}_{tipoUsuario.ToString().ToLower()}"
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Propietario, RolUsuario.Inquilino, RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile profilePhoto)
        {
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            if (profilePhoto == null || profilePhoto.Length == 0)
            {
                return Json(new { success = false, message = "No se seleccionó ningún archivo" });
            }

            var (success, message, filePath) = await _profilePhotoService.UploadProfilePhotoAsync(
                profilePhoto, usuarioId.Value, "user");

            if (success && !string.IsNullOrEmpty(filePath))
            {
                // Actualizar la foto de perfil en la base de datos
                var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId.Value);
                if (usuario != null)
                {
                    // Eliminar foto anterior si existe
                    if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                    {
                        _profilePhotoService.DeleteProfilePhoto(usuario.FotoPerfil);
                    }

                    // Actualizar usuario con nueva foto
                    var updateSuccess = await _usuarioService.UpdateProfilePhotoAsync(usuarioId.Value, filePath);
                    if (updateSuccess)
                    {
                        // Refrescar claims para mostrar la nueva foto inmediatamente
                        await RefreshUserClaimsAsync(usuarioId.Value);
                        
                        TempData["SuccessMessage"] = "Foto de perfil actualizada exitosamente";
                        return Json(new { success = true, message = "Foto subida exitosamente", photoUrl = filePath });
                    }
                    else
                    {
                        // Si falla la actualización, eliminar el archivo subido
                        _profilePhotoService.DeleteProfilePhoto(filePath);
                        return Json(new { success = false, message = "Error al actualizar la foto en la base de datos" });
                    }
                }
            }

            return Json(new { success = false, message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeMultipleRoles(RolUsuario.Propietario, RolUsuario.Inquilino, RolUsuario.Empleado, RolUsuario.Administrador)]
        public async Task<IActionResult> DeleteProfilePhoto()
        {
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return Json(new { success = false, message = "Usuario no autenticado" });
            }

            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId.Value);
                if (usuario != null)
                {
                    // Eliminar archivo físico si existe (no eliminar la imagen por defecto)
                    if (!string.IsNullOrEmpty(usuario.FotoPerfil) && 
                        !usuario.FotoPerfil.Contains("user_default.png"))
                    {
                        _profilePhotoService.DeleteProfilePhoto(usuario.FotoPerfil);
                    }

                    // Actualizar usuario para usar imagen por defecto (null en BD)
                    var updateSuccess = await _usuarioService.UpdateProfilePhotoAsync(usuarioId.Value, null!);
                    if (updateSuccess)
                    {
                        // Refrescar claims para mostrar la imagen por defecto inmediatamente
                        await RefreshUserClaimsAsync(usuarioId.Value);
                        
                        var defaultPhotoUrl = _profilePhotoService.GetDefaultProfilePhoto();
                        TempData["SuccessMessage"] = "Foto de perfil eliminada exitosamente";
                        return Json(new { success = true, message = "Foto eliminada exitosamente", photoUrl = defaultPhotoUrl });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Error al actualizar la foto en la base de datos" });
                    }
                }

                return Json(new { success = false, message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto de perfil para usuario {UserId}", usuarioId);
                return Json(new { success = false, message = "Error interno del servidor" });
            }
        }

        private async Task RefreshUserClaimsAsync(int usuarioId)
        {
            try
            {
                // Obtener usuario actualizado con la nueva foto
                var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId);
                if (usuario != null)
                {
                    // Crear nuevos claims con la información actualizada
                    var newClaimsPrincipal = await _authenticationService.CreateClaimsPrincipalAsync(usuario);
                    if (newClaimsPrincipal != null)
                    {
                        // Actualizar la autenticación con los nuevos claims
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newClaimsPrincipal);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar claims para usuario: {UsuarioId}", usuarioId);
            }
        }

        // GET: Auth/CambioContrasenaObligatorio
        [HttpGet]
        public async Task<IActionResult> CambioContrasenaObligatorio()
        {
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId.Value);
            if (usuario == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Si el usuario no requiere cambio, redirigir al perfil
            if (!usuario.RequiereCambioClave)
            {
                return RedirectToAction(nameof(Profile));
            }

            return View(usuario);
        }

        // POST: Auth/CambiarClave - Cambio de contraseña obligatorio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(string newPassword, string confirmPassword)
        {
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Las contraseñas no coinciden";
                return RedirectToAction(nameof(CambioContrasenaObligatorio));
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "La contraseña debe tener al menos 6 caracteres";
                return RedirectToAction(nameof(CambioContrasenaObligatorio));
            }

            try
            {
                var usuario = await _usuarioService.GetUsuarioByIdAsync(usuarioId.Value);
                if (usuario == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                // Cambiar contraseña
                usuario.ClaveHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                usuario.RequiereCambioClave = false;

                var success = await _usuarioRepository.UpdateAsync(usuario);

                if (success)
                {
                    _logger.LogInformation("Usuario {UsuarioId} cambió contraseña obligatoria exitosamente", usuarioId.Value);
                    TempData["SuccessMessage"] = "Contraseña cambiada exitosamente";
                    
                    // Redirigir según el rol
                    return usuario.Rol switch
                    {
                        RolUsuario.Administrador or RolUsuario.Empleado => RedirectToAction("Index", "Home"),
                        RolUsuario.Propietario => RedirectToAction("MisInmuebles", "Inmuebles"),
                        RolUsuario.Inquilino => RedirectToAction("MiContrato", "Contratos"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al cambiar la contraseña";
                    return RedirectToAction(nameof(CambioContrasenaObligatorio));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña obligatoria: {UsuarioId}", usuarioId.Value);
                TempData["ErrorMessage"] = "Error interno del servidor";
                return RedirectToAction(nameof(CambioContrasenaObligatorio));
            }
        }
    }
}
