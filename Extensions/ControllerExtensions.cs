using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaGarciaJesus.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Valida que el usuario tenga una sesión válida con UserRole
        /// </summary>
        public static bool HasValidSession(this Controller controller)
        {
            if (!controller.User.Identity?.IsAuthenticated == true)
                return false;

            var userRole = controller.HttpContext.Session.GetString("UserRole");
            var userId = controller.HttpContext.Session.GetString("UserId");

            return !string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(userId);
        }

        /// <summary>
        /// Obtiene el UserRole de la sesión de forma segura
        /// </summary>
        public static string? GetUserRole(this Controller controller)
        {
            if (!controller.HasValidSession())
                return null;

            return controller.HttpContext.Session.GetString("UserRole");
        }

        /// <summary>
        /// Obtiene el UserId de la sesión de forma segura
        /// </summary>
        public static int? GetSessionUserId(this Controller controller)
        {
            if (!controller.HasValidSession())
                return null;

            var userIdString = controller.HttpContext.Session.GetString("UserId");
            if (int.TryParse(userIdString, out var userId))
                return userId;

            return null;
        }

        /// <summary>
        /// Redirige al login si la sesión no es válida
        /// </summary>
        public static IActionResult? RedirectToLoginIfInvalidSession(this Controller controller)
        {
            if (!controller.HasValidSession())
            {
                return controller.RedirectToAction("Login", "Auth", new { message = "session_expired" });
            }

            return null;
        }
    }
}
