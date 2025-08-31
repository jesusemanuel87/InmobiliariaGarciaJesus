namespace InmobiliariaGarciaJesus.Models
{
    public class ConfiguracionAdminViewModel
    {
        public List<Configuracion> MesesMinimos { get; set; } = new();
        public Configuracion? MultaTerminacionTemprana { get; set; }
        public Configuracion? MultaTerminacionTardia { get; set; }
        public Configuracion? InteresVencimiento { get; set; }
        public Configuracion? InteresVencimientoExtendido { get; set; }
        public Configuracion? InteresVencimientoMensual { get; set; }
    }

    public class ConfiguracionUpdateModel
    {
        public string Clave { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public TipoConfiguracion Tipo { get; set; }
    }
}
