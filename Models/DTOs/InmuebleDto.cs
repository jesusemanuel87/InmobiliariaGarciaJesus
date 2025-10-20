using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InmobiliariaGarciaJesus.Models.DTOs
{
    /// <summary>
    /// DTO para información de inmueble
    /// </summary>
    public class InmuebleDto
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public int TipoId { get; set; }
        public string TipoNombre { get; set; } = string.Empty;
        public int Ambientes { get; set; }
        public decimal? Superficie { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        
        /// <summary>
        /// Estado del inmueble: Activo o Inactivo (controlado por el propietario)
        /// </summary>
        public string Estado { get; set; } = string.Empty;
        
        /// <summary>
        /// Disponibilidad del inmueble basada en contratos:
        /// - Disponible: Sin contrato activo/reservado
        /// - Reservado: Con contrato en estado Reservado
        /// - No Disponible: Con contrato activo
        /// </summary>
        public string Disponibilidad { get; set; } = string.Empty;
        
        public decimal? Precio { get; set; }
        public string Uso { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string? ImagenPortadaUrl { get; set; }
        public List<InmuebleImagenDto> Imagenes { get; set; } = new();
    }

    /// <summary>
    /// DTO para imagen de inmueble
    /// </summary>
    public class InmuebleImagenDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaCompleta { get; set; } = string.Empty;
        public bool EsPortada { get; set; }
        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo inmueble
    /// </summary>
    public class CrearInmuebleDto
    {
        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string Direccion { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La localidad no puede exceder 100 caracteres")]
        public string? Localidad { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres")]
        public string? Provincia { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public int TipoId { get; set; }

        [Required(ErrorMessage = "Los ambientes son obligatorios")]
        [Range(1, 100, ErrorMessage = "Los ambientes deben estar entre 1 y 100")]
        public int Ambientes { get; set; }

        [Range(0, 999999, ErrorMessage = "La superficie debe ser mayor a 0")]
        public decimal? Superficie { get; set; }

        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El uso es obligatorio")]
        public int Uso { get; set; } // 0=Residencial, 1=Comercial, 2=Industrial

        // La imagen se enviará en un campo separado como base64
        public string? ImagenBase64 { get; set; }
        public string? ImagenNombre { get; set; }
    }

    /// <summary>
    /// DTO para actualizar estado de inmueble (Activo/Inactivo)
    /// Acepta tanto formato boolean como string por compatibilidad
    /// </summary>
    public class ActualizarEstadoInmuebleDto
    {
        /// <summary>
        /// Estado como boolean (true=Activo, false=Inactivo)
        /// </summary>
        public bool? Activo { get; set; }
        
        /// <summary>
        /// Estado como string ("Activo" o "Inactivo")
        /// Para compatibilidad con apps que envían string
        /// </summary>
        [JsonPropertyName("estado")]
        public string? Estado { get; set; }
        
        /// <summary>
        /// Obtiene el estado como enum, priorizando el campo que tenga valor
        /// </summary>
        public EstadoInmueble ObtenerEstado()
        {
            // Priorizar el campo Estado (string) si viene
            if (!string.IsNullOrEmpty(Estado))
            {
                return Estado.Trim().Equals("Activo", StringComparison.OrdinalIgnoreCase)
                    ? EstadoInmueble.Activo
                    : EstadoInmueble.Inactivo;
            }
            
            // Si no, usar el campo Activo (boolean)
            return Activo.GetValueOrDefault() ? EstadoInmueble.Activo : EstadoInmueble.Inactivo;
        }
    }
}
