using InmobiliariaGarciaJesus.Services;
using System.Security.Claims;

namespace InmobiliariaGarciaJesus.Middleware
{
    public class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequirePasswordChangeMiddleware> _logger;

        // Rutas permitidas cuando el usuario requiere cambio de contraseña
        private static readonly string[] AllowedPaths = new[]
        {
            "/auth/cambiocontrasenaobligatorio",
            "/auth/cambiarclave",
            "/auth/logout",
            "/css/",
            "/js/",
            "/lib/",
            "/images/",
            "/uploads/"
        };

        public RequirePasswordChangeMiddleware(RequestDelegate next, ILogger<RequirePasswordChangeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UsuarioService usuarioService)
        {
            // Solo aplicar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var usuarioIdClaim = context.User.FindFirst("UsuarioId");
                
                if (usuarioIdClaim != null && int.TryParse(usuarioIdClaim.Value, out int usuarioId))
                {
                    try
                    {
                        var usuario = await usuarioService.GetUsuarioByIdAsync(usuarioId);
                        
                        // Si el usuario requiere cambio de contraseña
                        if (usuario != null && usuario.RequiereCambioClave)
                        {
                            var path = context.Request.Path.Value?.ToLower() ?? "";
                            
                            // Verificar si la ruta actual está permitida
                            bool isAllowedPath = AllowedPaths.Any(allowedPath => 
                                path.StartsWith(allowedPath, StringComparison.OrdinalIgnoreCase));

                            if (!isAllowedPath)
                            {
                                _logger.LogInformation(
                                    "Usuario {UsuarioId} requiere cambio de contraseña. Redirigiendo desde {Path}",
                                    usuarioId, path);
                                
                                context.Response.Redirect("/Auth/CambioContrasenaObligatorio");
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al verificar RequiereCambioClave para usuario {UsuarioId}", usuarioId);
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method para agregar el middleware
    public static class RequirePasswordChangeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequirePasswordChange(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequirePasswordChangeMiddleware>();
        }
    }
}
