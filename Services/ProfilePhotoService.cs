using InmobiliariaGarciaJesus.Models;

namespace InmobiliariaGarciaJesus.Services
{
    public class ProfilePhotoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProfilePhotoService> _logger;
        private const string ProfilePhotosFolder = "uploads/profiles";
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public ProfilePhotoService(IWebHostEnvironment environment, ILogger<ProfilePhotoService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string? FilePath)> UploadProfilePhotoAsync(IFormFile file, int userId, string userType)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return (false, "No se seleccionó ningún archivo", null);
                }

                // Validar tamaño del archivo
                if (file.Length > MaxFileSize)
                {
                    return (false, "El archivo es demasiado grande. Máximo 5MB permitido", null);
                }

                // Validar extensión
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    return (false, "Formato de archivo no permitido. Use: jpg, jpeg, png, gif, webp", null);
                }

                // Crear directorio si no existe
                var uploadsPath = Path.Combine(_environment.WebRootPath, ProfilePhotosFolder);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generar nombre único para el archivo
                var fileName = $"{userType}_{userId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Retornar ruta relativa para la base de datos
                var relativePath = $"/{ProfilePhotosFolder}/{fileName}".Replace("\\", "/");
                
                _logger.LogInformation("Foto de perfil subida exitosamente: {FilePath} para usuario {UserId}", relativePath, userId);
                return (true, "Foto de perfil subida exitosamente", relativePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al subir foto de perfil para usuario {UserId}", userId);
                return (false, "Error interno al subir la foto", null);
            }
        }

        public bool DeleteProfilePhoto(string? filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return true;

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Foto de perfil eliminada: {FilePath}", filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar foto de perfil: {FilePath}", filePath);
                return false;
            }
        }

        public string GetDefaultProfilePhoto(RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.Administrador => "/images/default-admin.png",
                RolUsuario.Empleado => "/images/default-employee.png",
                RolUsuario.Propietario => "/images/default-owner.png",
                RolUsuario.Inquilino => "/images/default-tenant.png",
                _ => "/images/default-user.png"
            };
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension) && file.Length <= MaxFileSize;
        }
    }
}
