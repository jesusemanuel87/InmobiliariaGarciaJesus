using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace InmobiliariaGarciaJesus.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionValidationMiddleware> _logger;

        public SessionValidationMiddleware(RequestDelegate next, ILogger<SessionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Solo validar si el usuario está autenticado
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Verificar si la sesión tiene los datos necesarios
                var userRole = context.Session.GetString("UserRole");
                var userId = context.Session.GetString("UserId");

                // Si el usuario está autenticado pero no tiene datos de sesión, cerrar sesión
                if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Usuario autenticado sin datos de sesión válidos. Cerrando sesión automáticamente.");
                    
                    // Limpiar la sesión
                    context.Session.Clear();
                    
                    // Cerrar sesión de autenticación
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    
                    // Si es una petición AJAX, devolver error 401
                    if (IsAjaxRequest(context.Request))
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\":\"Sesión expirada. Por favor, inicie sesión nuevamente.\"}");
                        return;
                    }
                    
                    // Si no es AJAX, redirigir al login
                    context.Response.Redirect("/Auth/Login?message=session_expired");
                    return;
                }
            }

            await _next(context);
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return request.Headers.ContainsKey("X-Requested-With") && 
                   request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   request.Headers.ContainsKey("Accept") && 
                   request.Headers["Accept"].ToString().Contains("application/json");
        }
    }
}
