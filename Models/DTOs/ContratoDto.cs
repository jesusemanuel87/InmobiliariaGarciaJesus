namespace InmobiliariaGarciaJesus.Models.DTOs
{
    /// <summary>
    /// DTO para información de contrato
    /// </summary>
    public class ContratoDto
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal Precio { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string? MotivoCancelacion { get; set; }
        public DateTime? FechaFinalizacionReal { get; set; }
        public decimal? MultaFinalizacion { get; set; }
        public int? MesesAdeudados { get; set; }
        public decimal? ImporteAdeudado { get; set; }
        
        // Datos del inmueble
        public InmuebleContratoDto Inmueble { get; set; } = null!;
        
        // Datos del inquilino
        public InquilinoContratoDto Inquilino { get; set; } = null!;
        
        // Pagos asociados
        public List<PagoDto> Pagos { get; set; } = new();
    }

    /// <summary>
    /// DTO simplificado de inmueble para contratos
    /// </summary>
    public class InmuebleContratoDto
    {
        public int Id { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public int Ambientes { get; set; }
        public string? ImagenPortadaUrl { get; set; }
    }

    /// <summary>
    /// DTO simplificado de inquilino para contratos
    /// </summary>
    public class InquilinoContratoDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
