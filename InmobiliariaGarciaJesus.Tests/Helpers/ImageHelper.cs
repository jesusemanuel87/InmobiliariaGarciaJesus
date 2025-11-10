namespace InmobiliariaGarciaJesus.Tests.Helpers
{
    /// <summary>
    /// Helper para generar imágenes Base64 para tests sin archivos externos
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Genera una imagen PNG válida de 1x1 pixel en Base64
        /// Es una imagen mínima pero válida que puede ser procesada por el sistema
        /// </summary>
        public static string GenerarImagenBase64()
        {
            // PNG mínimo válido (1x1 pixel rojo)
            // Este es un PNG real de 67 bytes que cualquier librería puede procesar
            byte[] pngBytes = new byte[]
            {
                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
                0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, // IHDR chunk start
                0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, // Width: 1, Height: 1
                0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, // Color type: RGB
                0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, // IDAT chunk
                0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, // Pixel data
                0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, // ...
                0xB4, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, // IEND chunk
                0x44, 0xAE, 0x42, 0x60, 0x82                    // PNG end
            };

            return Convert.ToBase64String(pngBytes);
        }

        /// <summary>
        /// Genera un nombre de archivo único para tests
        /// </summary>
        public static string GenerarNombreArchivo(string extension = "png")
        {
            return $"test-image-{Guid.NewGuid()}.{extension}";
        }

        /// <summary>
        /// Genera imagen Base64 con nombre único (ambos en una llamada)
        /// </summary>
        public static (string base64, string nombre) GenerarImagenConNombre()
        {
            return (GenerarImagenBase64(), GenerarNombreArchivo());
        }
    }
}
