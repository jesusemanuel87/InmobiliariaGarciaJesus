using InmobiliariaGarciaJesus.Models;
using System.Security.Claims;

namespace InmobiliariaGarciaJesus.Services
{
    public class AuthenticationService
    {
        private readonly UsuarioService _usuarioService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(UsuarioService usuarioService, ILogger<AuthenticationService> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal?> CreateClaimsPrincipalAsync(Usuario usuario)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new("UsuarioId", usuario.Id.ToString()), // Para middleware
                    new(ClaimTypes.Name, usuario.NombreUsuario),
                    new(ClaimTypes.Email, usuario.Email),
                    new(ClaimTypes.Role, usuario.Rol.ToString()),
                    new("FullName", usuario.NombreCompleto),
                    new("RoleDescription", usuario.RolDescripcion),
                    new("RequiereCambioClave", usuario.RequiereCambioClave.ToString())
                };

                // Agregar foto de perfil si existe
                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    claims.Add(new Claim("FotoPerfil", usuario.FotoPerfil));
                }

                // Agregar claims específicos según el rol
                switch (usuario.Rol)
                {
                    case RolUsuario.Propietario:
                        if (usuario.PropietarioId.HasValue)
                        {
                            claims.Add(new Claim("PropietarioId", usuario.PropietarioId.Value.ToString()));
                        }
                        break;
                    case RolUsuario.Inquilino:
                        if (usuario.InquilinoId.HasValue)
                        {
                            claims.Add(new Claim("InquilinoId", usuario.InquilinoId.Value.ToString()));
                        }
                        break;
                    case RolUsuario.Empleado:
                    case RolUsuario.Administrador:
                        if (usuario.EmpleadoId.HasValue)
                        {
                            claims.Add(new Claim("EmpleadoId", usuario.EmpleadoId.Value.ToString()));
                            if (usuario.Empleado != null)
                            {
                                claims.Add(new Claim("EmpleadoRol", usuario.Empleado.Rol.ToString()));
                            }
                        }
                        break;
                }

                // Agregar permisos específicos
                AddPermissionClaims(claims, usuario.Rol);

                var identity = new ClaimsIdentity(claims, "Cookie");
                return new ClaimsPrincipal(identity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ClaimsPrincipal para usuario: {UsuarioId}", usuario.Id);
                return null;
            }
        }

        private static void AddPermissionClaims(List<Claim> claims, RolUsuario rol)
        {
            switch (rol)
            {
                case RolUsuario.Administrador:
                    claims.Add(new Claim("Permission", "CanDelete"));
                    claims.Add(new Claim("Permission", "CanManageUsers"));
                    claims.Add(new Claim("Permission", "CanAccessConfiguration"));
                    claims.Add(new Claim("Permission", "CanViewAllData"));
                    claims.Add(new Claim("Permission", "CanEditAllData"));
                    break;

                case RolUsuario.Empleado:
                    claims.Add(new Claim("Permission", "CanViewAllData"));
                    claims.Add(new Claim("Permission", "CanEditAllData"));
                    break;

                case RolUsuario.Propietario:
                    claims.Add(new Claim("Permission", "CanViewOwnProperties"));
                    claims.Add(new Claim("Permission", "CanEditOwnProperties"));
                    claims.Add(new Claim("Permission", "CanViewOwnContracts"));
                    claims.Add(new Claim("Permission", "CanViewOwnPayments"));
                    break;

                case RolUsuario.Inquilino:
                    claims.Add(new Claim("Permission", "CanViewOwnContract"));
                    claims.Add(new Claim("Permission", "CanViewOwnPayments"));
                    break;
            }
        }

        public static bool HasPermission(ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("Permission", permission);
        }

        public static bool IsAdministrador(ClaimsPrincipal user)
        {
            return user.IsInRole(RolUsuario.Administrador.ToString());
        }

        public static bool IsEmpleado(ClaimsPrincipal user)
        {
            return user.IsInRole(RolUsuario.Empleado.ToString()) || IsAdministrador(user);
        }

        public static bool IsPropietario(ClaimsPrincipal user)
        {
            return user.IsInRole(RolUsuario.Propietario.ToString());
        }

        public static bool IsInquilino(ClaimsPrincipal user)
        {
            return user.IsInRole(RolUsuario.Inquilino.ToString());
        }

        public static int? GetUsuarioId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public static int? GetPropietarioId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("PropietarioId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public static int? GetInquilinoId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("InquilinoId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }

        public static int? GetEmpleadoId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("EmpleadoId");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
        }
    }
}
