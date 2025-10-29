using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InmobiliariaGarciaJesus.Models
{
    /// <summary>
    /// Log de sincronizaciones con API Georef
    /// </summary>
    [Table("georef_sync_log")]
    public class GeorefSyncLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoSincronizacion { get; set; } = string.Empty;

        public DateTime FechaSincronizacion { get; set; } = DateTime.Now;

        public bool Exitosa { get; set; }

        public int? CantidadRegistros { get; set; }

        [Column(TypeName = "text")]
        public string? MensajeError { get; set; }
    }
}
