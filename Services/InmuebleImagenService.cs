using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public interface IInmuebleImagenService
    {
        Task<IEnumerable<InmuebleImagen>> GetImagenesByInmuebleAsync(int inmuebleId);
        Task<InmuebleImagen?> GetPortadaByInmuebleAsync(int inmuebleId);
        Task<InmuebleImagen> GuardarImagenAsync(int inmuebleId, IFormFile archivo, string? descripcion = null, bool esPortada = false);
        Task<bool> EliminarImagenAsync(int imagenId);
        Task<bool> EstablecerPortadaAsync(int imagenId, int inmuebleId);
        Task<string> CrearDirectorioInmuebleAsync(int inmuebleId);
    }

    public class InmuebleImagenService : IInmuebleImagenService
    {
        private readonly InmuebleImagenRepository _imagenRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _tamanoMaximo = 5 * 1024 * 1024; // 5MB

        public InmuebleImagenService(InmuebleImagenRepository imagenRepository, IWebHostEnvironment environment)
        {
            _imagenRepository = imagenRepository;
            _environment = environment;
        }

        public async Task<IEnumerable<InmuebleImagen>> GetImagenesByInmuebleAsync(int inmuebleId)
        {
            return await _imagenRepository.GetByInmuebleIdAsync(inmuebleId);
        }

        public async Task<InmuebleImagen?> GetPortadaByInmuebleAsync(int inmuebleId)
        {
            return await _imagenRepository.GetPortadaByInmuebleIdAsync(inmuebleId);
        }

        public async Task<InmuebleImagen> GuardarImagenAsync(int inmuebleId, IFormFile archivo, string? descripcion = null, bool esPortada = false)
        {
            // Validaciones
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("El archivo es requerido");

            if (archivo.Length > _tamanoMaximo)
                throw new ArgumentException($"El archivo no puede superar {_tamanoMaximo / (1024 * 1024)}MB");

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!_extensionesPermitidas.Contains(extension))
                throw new ArgumentException($"Tipo de archivo no permitido. Extensiones válidas: {string.Join(", ", _extensionesPermitidas)}");

            // Crear directorio si no existe
            var directorioInmueble = await CrearDirectorioInmuebleAsync(inmuebleId);
            
            // Generar nombre único para el archivo
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(directorioInmueble, nombreArchivo);
            var rutaRelativa = $"/uploads/inmuebles/{inmuebleId}/{nombreArchivo}";

            // Guardar archivo físico
            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Si es la primera imagen del inmueble, marcarla como portada automáticamente
            var imagenesExistentes = await _imagenRepository.GetByInmuebleIdAsync(inmuebleId);
            if (!imagenesExistentes.Any())
            {
                esPortada = true;
            }

            // Crear registro en base de datos
            var imagen = new InmuebleImagen
            {
                InmuebleId = inmuebleId,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaRelativa,
                EsPortada = esPortada,
                Descripcion = descripcion,
                TamanoBytes = archivo.Length,
                TipoMime = archivo.ContentType
            };

            var id = await _imagenRepository.CreateAsync(imagen);
            imagen.Id = id;
            return imagen;
        }

        public async Task<bool> EliminarImagenAsync(int imagenId)
        {
            var imagen = await _imagenRepository.GetByIdAsync(imagenId);
            if (imagen == null) return false;

            // Eliminar archivo físico
            var rutaCompleta = Path.Combine(_environment.WebRootPath, imagen.RutaArchivo.TrimStart('/'));
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }

            // Eliminar registro de base de datos
            var eliminado = await _imagenRepository.DeleteAsync(imagenId);

            // Si era la portada, establecer otra imagen como portada
            if (eliminado && imagen.EsPortada)
            {
                var imagenesRestantes = await _imagenRepository.GetByInmuebleIdAsync(imagen.InmuebleId);
                var primeraImagen = imagenesRestantes.FirstOrDefault();
                if (primeraImagen != null)
                {
                    await _imagenRepository.SetPortadaAsync(primeraImagen.Id, imagen.InmuebleId);
                }
            }

            return eliminado;
        }

        public async Task<bool> EstablecerPortadaAsync(int imagenId, int inmuebleId)
        {
            return await _imagenRepository.SetPortadaAsync(imagenId, inmuebleId);
        }

        public Task<string> CrearDirectorioInmuebleAsync(int inmuebleId)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var inmueblesPath = Path.Combine(uploadsPath, "inmuebles");
            var inmuebleSpecificPath = Path.Combine(inmueblesPath, inmuebleId.ToString());

            // Crear directorios si no existen
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);
                
            if (!Directory.Exists(inmueblesPath))
                Directory.CreateDirectory(inmueblesPath);
                
            if (!Directory.Exists(inmuebleSpecificPath))
                Directory.CreateDirectory(inmuebleSpecificPath);

            return Task.FromResult(inmuebleSpecificPath);
        }
    }
}
