using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Attributes
{
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly RolUsuario[] _allowedRoles;

        public AuthorizeRoleAttribute(params RolUsuario[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Verificar si el usuario tiene uno de los roles permitidos
            var userRole = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || !Enum.TryParse<RolUsuario>(userRole, out var currentRole))
            {
                context.Result = new ForbidResult();
                return;
            }

            if (!_allowedRoles.Contains(currentRole))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    public class AuthorizePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _requiredPermissions;

        public AuthorizePermissionAttribute(params string[] requiredPermissions)
        {
            _requiredPermissions = requiredPermissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Verificar si el usuario tiene al menos uno de los permisos requeridos
            var hasPermission = _requiredPermissions.Any(permission =>
                AuthenticationService.HasPermission(context.HttpContext.User, permission));

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    // Atributos específicos para roles comunes
    public class AuthorizeAdminAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeAdminAttribute() : base(RolUsuario.Administrador) { }
    }

    public class AuthorizeEmpleadoAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeEmpleadoAttribute() : base(RolUsuario.Empleado, RolUsuario.Administrador) { }
    }

    public class AuthorizePropietarioAttribute : AuthorizeRoleAttribute
    {
        public AuthorizePropietarioAttribute() : base(RolUsuario.Propietario) { }
    }

    public class AuthorizeInquilinoAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeInquilinoAttribute() : base(RolUsuario.Inquilino) { }
    }

    // Atributo para permitir múltiples roles específicos
    public class AuthorizeMultipleRolesAttribute : AuthorizeRoleAttribute
    {
        public AuthorizeMultipleRolesAttribute(params RolUsuario[] roles) : base(roles) { }
    }
}
