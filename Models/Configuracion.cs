using System.ComponentModel.DataAnnotations;

namespace InmobiliariaGarciaJesus.Models
{
    public class Configuracion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La clave es requerida")]
        [StringLength(100, ErrorMessage = "La clave no puede exceder los 100 caracteres")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "El valor es requerido")]
        [StringLength(500, ErrorMessage = "El valor no puede exceder los 500 caracteres")]
        public string Valor { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string? Descripcion { get; set; }

        [Required]
        public TipoConfiguracion Tipo { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaModificacion { get; set; } = DateTime.Now;
    }

    public enum TipoConfiguracion
    {
        MesesMinimos = 1,
        MultaTerminacionTemprana = 2,
        MultaTerminacionTardia = 3,
        InteresVencimiento = 4,
        InteresVencimientoExtendido = 5,
        InteresVencimientoMensual = 6
    }
}
