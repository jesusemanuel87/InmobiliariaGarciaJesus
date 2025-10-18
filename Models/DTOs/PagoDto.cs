namespace InmobiliariaGarciaJesus.Models.DTOs
{
    /// <summary>
    /// DTO para información de pago
    /// </summary>
    public class PagoDto
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public DateTime? FechaPago { get; set; }
        public int ContratoId { get; set; }
        public decimal Importe { get; set; }
        public decimal Intereses { get; set; }
        public decimal Multas { get; set; }
        public decimal TotalAPagar { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? MetodoPago { get; set; }
        public string? Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
