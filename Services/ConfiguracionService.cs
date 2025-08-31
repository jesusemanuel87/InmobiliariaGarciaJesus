using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public interface IConfiguracionService
    {
        Task<string> GetConfiguracionValueAsync(string clave, string defaultValue = "");
        Task<int> GetConfiguracionIntAsync(string clave, int defaultValue = 0);
        Task<decimal> GetConfiguracionDecimalAsync(string clave, decimal defaultValue = 0);
        Task<bool> GetConfiguracionBoolAsync(string clave, bool defaultValue = false);
        Task<List<int>> GetMesesMinimoHabilitadosAsync();
        Task<bool> UpdateConfiguracionAsync(string clave, string valor, string? descripcion = null, TipoConfiguracion? tipo = null);
    }

    public class ConfiguracionService : IConfiguracionService
    {
        private readonly ConfiguracionRepository _configuracionRepository;

        public ConfiguracionService(IRepository<Configuracion> configuracionRepository)
        {
            _configuracionRepository = (ConfiguracionRepository)configuracionRepository;
        }

        public async Task<string> GetConfiguracionValueAsync(string clave, string defaultValue = "")
        {
            var config = await _configuracionRepository.GetByClaveAsync(clave);
            return config?.Valor ?? defaultValue;
        }

        public async Task<int> GetConfiguracionIntAsync(string clave, int defaultValue = 0)
        {
            var valor = await GetConfiguracionValueAsync(clave, defaultValue.ToString());
            return int.TryParse(valor, out var result) ? result : defaultValue;
        }

        public async Task<decimal> GetConfiguracionDecimalAsync(string clave, decimal defaultValue = 0)
        {
            var valor = await GetConfiguracionValueAsync(clave, defaultValue.ToString());
            return decimal.TryParse(valor, out var result) ? result : defaultValue;
        }

        public async Task<bool> GetConfiguracionBoolAsync(string clave, bool defaultValue = false)
        {
            var valor = await GetConfiguracionValueAsync(clave, defaultValue.ToString());
            return bool.TryParse(valor, out var result) ? result : defaultValue;
        }

        public async Task<List<int>> GetMesesMinimoHabilitadosAsync()
        {
            var mesesConfig = await _configuracionRepository.GetByTipoAsync(TipoConfiguracion.MesesMinimos);
            var mesesHabilitados = new List<int>();

            foreach (var config in mesesConfig)
            {
                if (bool.TryParse(config.Valor, out var habilitado) && habilitado)
                {
                    // Extraer el número de meses de la clave (ej: "MESES_MINIMOS_12" -> 12)
                    var partes = config.Clave.Split('_');
                    if (partes.Length > 2 && int.TryParse(partes.Last(), out var meses))
                    {
                        mesesHabilitados.Add(meses);
                    }
                }
            }

            return mesesHabilitados.OrderBy(m => m).ToList();
        }

        public async Task<bool> UpdateConfiguracionAsync(string clave, string valor, string? descripcion = null, TipoConfiguracion? tipo = null)
        {
            var config = await _configuracionRepository.GetByClaveAsync(clave);
            
            if (config != null)
            {
                config.Valor = valor;
                config.FechaModificacion = DateTime.Now;
                if (!string.IsNullOrEmpty(descripcion))
                    config.Descripcion = descripcion;
                
                return await _configuracionRepository.UpdateAsync(config);
            }
            else if (tipo.HasValue)
            {
                var nuevaConfig = new Configuracion
                {
                    Clave = clave,
                    Valor = valor,
                    Descripcion = descripcion,
                    Tipo = tipo.Value,
                    FechaCreacion = DateTime.Now,
                    FechaModificacion = DateTime.Now
                };
                
                var id = await _configuracionRepository.CreateAsync(nuevaConfig);
                return id > 0;
            }

            return false;
        }
    }
}
