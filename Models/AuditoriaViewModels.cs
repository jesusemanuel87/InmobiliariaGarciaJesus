using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class AuditoriaViewModel
    {
        [Display(Name = "Entidad")]
        public string TipoEntidad { get; set; } = string.Empty;

        [Display(Name = "ID de Entidad")]
        public int EntidadId { get; set; }

        [Display(Name = "Fecha de Creación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Creado Por")]
        public string? CreadoPor { get; set; }

        [Display(Name = "Usuario Creador")]
        public string? UsuarioCreador { get; set; }

        [Display(Name = "Fecha de Modificación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaModificacion { get; set; }

        [Display(Name = "Modificado Por")]
        public string? ModificadoPor { get; set; }

        [Display(Name = "Usuario Modificador")]
        public string? UsuarioModificador { get; set; }

        [Display(Name = "Acción Realizada")]
        public string? AccionRealizada { get; set; }

        [Display(Name = "Estado Actual")]
        public string? EstadoActual { get; set; }

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }

    public class ContratoAuditoriaViewModel : AuditoriaViewModel
    {
        [Display(Name = "Número de Contrato")]
        public int NumeroContrato { get; set; }

        [Display(Name = "Inquilino")]
        public string? NombreInquilino { get; set; }

        [Display(Name = "Inmueble")]
        public string? DireccionInmueble { get; set; }

        [Display(Name = "Fecha de Terminación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaTerminacion { get; set; }

        [Display(Name = "Terminado Por")]
        public string? TerminadoPor { get; set; }

        [Display(Name = "Usuario que Terminó")]
        public string? UsuarioTerminador { get; set; }
    }

    public class PagoAuditoriaViewModel : AuditoriaViewModel
    {
        [Display(Name = "Número de Pago")]
        public int NumeroPago { get; set; }

        [Display(Name = "Contrato")]
        public int ContratoId { get; set; }

        [Display(Name = "Importe")]
        [DataType(DataType.Currency)]
        public decimal Importe { get; set; }

        [Display(Name = "Fecha de Anulación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaAnulacion { get; set; }

        [Display(Name = "Anulado Por")]
        public string? AnuladoPor { get; set; }

        [Display(Name = "Usuario que Anuló")]
        public string? UsuarioAnulador { get; set; }

        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; }
    }
}
