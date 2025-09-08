using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Services;
using AuthService = InmobiliariaGarciaJesus.Services.AuthenticationService;
using InmobiliariaGarciaJesus.Attributes;
using System.Security.Claims;

namespace InmobiliariaGarciaJesus.Controllers
{
    public class AuthController : Controller
    {
        private readonly UsuarioService _usuarioService;
        private readonly AuthService _authenticationService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UsuarioService usuarioService,
            AuthService authenticationService,
            ILogger<AuthController> logger)
        {
            _usuarioService = usuarioService;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
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

            _logger.LogInformation("Usuario logueado exitosamente: {Email}", model.Email);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirigir según el rol del usuario
            return usuario.Rol switch
            {
                RolUsuario.Administrador or RolUsuario.Empleado => RedirectToAction("Index", "Propietarios"),
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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar campos requeridos según el tipo de usuario
            if (model.TipoUsuario == RolUsuario.Propietario || model.TipoUsuario == RolUsuario.Inquilino)
            {
                if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Apellido) ||
                    string.IsNullOrEmpty(model.Dni) || string.IsNullOrEmpty(model.Telefono))
                {
                    ModelState.AddModelError(string.Empty, "Todos los campos personales son obligatorios");
                    return View(model);
                }
            }

            var (success, message, usuarioId) = await _usuarioService.RegisterUsuarioAsync(model);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["SuccessMessage"] = "Usuario registrado exitosamente. Puede iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
            var usuarioId = AuthService.GetUsuarioId(User);
            if (!usuarioId.HasValue || usuarioId.Value != model.UsuarioId)
            {
                return RedirectToAction(nameof(Login));
            }

            var (success, message) = await _usuarioService.SwitchRoleAsync(model.UsuarioId, model.NuevoRol);

            if (success)
            {
                // Cerrar sesión y volver a iniciar con el nuevo rol
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                var usuario = await _usuarioService.GetUsuarioByIdAsync(model.UsuarioId);
                if (usuario != null)
                {
                    var claimsPrincipal = await _authenticationService.CreateClaimsPrincipalAsync(usuario);
                    if (claimsPrincipal != null)
                    {
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                    }
                }

                TempData["SuccessMessage"] = message;
                
                // Redirigir según el nuevo rol
                return model.NuevoRol switch
                {
                    RolUsuario.Administrador or RolUsuario.Empleado => RedirectToAction("Index", "Propietarios"),
                    RolUsuario.Propietario => RedirectToAction("MisInmuebles", "Inmuebles"),
                    RolUsuario.Inquilino => RedirectToAction("MiContrato", "Contratos"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction(nameof(Profile));
            }
        }

        // Método para verificar si un email está disponible (AJAX)
        [HttpPost]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            var exists = await _usuarioService.GetUsuarioByEmailAsync(email) != null;
            return Json(new { available = !exists });
        }

        // Método para verificar si un username está disponible (AJAX)
        [HttpPost]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            var exists = await _usuarioService.GetUsuarioByUsernameAsync(username) != null;
            return Json(new { available = !exists });
        }
    }
}
