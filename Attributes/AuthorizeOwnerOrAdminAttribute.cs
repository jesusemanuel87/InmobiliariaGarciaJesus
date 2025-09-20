using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Attributes
{
    /// <summary>
    /// Atributo que permite acceso solo a administradores/empleados o al propietario del recurso
    /// </summary>
    public class AuthorizeOwnerOrAdminAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _resourceIdParameter;
        private readonly string _ownerIdProperty;

        public AuthorizeOwnerOrAdminAttribute(string resourceIdParameter = "id", string ownerIdProperty = "PropietarioId")
        {
            _resourceIdParameter = resourceIdParameter;
            _ownerIdProperty = ownerIdProperty;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var userRole = context.HttpContext.Session.GetString("UserRole");
            var userId = context.HttpContext.Session.GetString("UserId");

            // Si es Administrador o Empleado, permitir acceso completo
            if (userRole == "Administrador" || userRole == "Empleado")
            {
                return;
            }

            // Si es Propietario, verificar que sea el dueño del recurso
            if (userRole == "Propietario")
            {
                if (!ValidateOwnership(context, userId))
                {
                    context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                    return;
                }
                return;
            }

            // Para otros roles, denegar acceso
            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }

        private bool ValidateOwnership(AuthorizationFilterContext context, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            // Obtener el ID del recurso desde los parámetros de la ruta
            if (!context.RouteData.Values.TryGetValue(_resourceIdParameter, out var resourceIdObj) ||
                !int.TryParse(resourceIdObj?.ToString(), out var resourceId))
            {
                return false;
            }

            try
            {
                // Obtener el servicio del repositorio desde el contenedor de dependencias
                var serviceProvider = context.HttpContext.RequestServices;
                
                // Para inmuebles, verificar que el propietario sea el correcto
                if (context.ActionDescriptor.RouteValues["controller"] == "Inmuebles")
                {
                    var inmuebleRepo = serviceProvider.GetService<IRepository<Inmueble>>();
                    if (inmuebleRepo != null)
                    {
                        var inmueble = inmuebleRepo.GetByIdAsync(resourceId).Result;
                        if (inmueble != null)
                        {
                            // Obtener el propietario asociado al usuario
                            var propietarioRepo = serviceProvider.GetService<IRepository<Propietario>>();
                            var usuarioRepo = serviceProvider.GetService<IRepository<Usuario>>();
                            
                            if (propietarioRepo != null && usuarioRepo != null)
                            {
                                var usuario = usuarioRepo.GetAllAsync().Result
                                    .FirstOrDefault(u => u.Id.ToString() == userId);
                                
                                if (usuario?.PropietarioId != null)
                                {
                                    return inmueble.PropietarioId == usuario.PropietarioId;
                                }
                            }
                        }
                    }
                }

                // Para contratos, verificar que el propietario tenga el inmueble asociado
                if (context.ActionDescriptor.RouteValues["controller"] == "Contratos")
                {
                    var contratoRepo = serviceProvider.GetService<IRepository<Contrato>>();
                    if (contratoRepo != null)
                    {
                        var contrato = contratoRepo.GetByIdAsync(resourceId).Result;
                        if (contrato != null)
                        {
                            var inmuebleRepo = serviceProvider.GetService<IRepository<Inmueble>>();
                            var usuarioRepo = serviceProvider.GetService<IRepository<Usuario>>();
                            
                            if (inmuebleRepo != null && usuarioRepo != null)
                            {
                                var inmueble = inmuebleRepo.GetByIdAsync(contrato.InmuebleId).Result;
                                var usuario = usuarioRepo.GetAllAsync().Result
                                    .FirstOrDefault(u => u.Id.ToString() == userId);
                                
                                if (inmueble != null && usuario?.PropietarioId != null)
                                {
                                    return inmueble.PropietarioId == usuario.PropietarioId;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
