using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InmobiliariaGarciaJesus.Services
{
    /// <summary>
    /// Servicio para manejar provincias y localidades con fallback a BD local
    /// </summary>
    public class GeorefService
    {
        private readonly InmobiliariaDbContext _context;
        private readonly ILogger<GeorefService> _logger;
        private readonly HttpClient _httpClient;
        private const string GEOREF_API_BASE = "https://apis.datos.gob.ar/georef/api";

        public GeorefService(
            InmobiliariaDbContext context,
            ILogger<GeorefService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10); // Timeout corto para no bloquear
        }

        #region Obtener Datos (con Fallback Automático)

        /// <summary>
        /// Obtiene provincias: primero desde BD local, sincroniza si es necesario
        /// </summary>
        public async Task<List<ProvinciaDto>> ObtenerProvinciasAsync()
        {
            try
            {
                // 1. Obtener desde BD local
                var provinciasLocal = await _context.GeorefProvincias
                    .OrderBy(p => p.Nombre)
                    .Select(p => new ProvinciaDto
                    {
                        Id = p.GeorefId,
                        Nombre = p.Nombre
                    })
                    .ToListAsync();

                if (provinciasLocal.Any())
                {
                    _logger.LogInformation($"Provincias cargadas desde BD local: {provinciasLocal.Count}");
                    
                    // Sincronizar en background si hace más de 30 días
                    _ = Task.Run(() => SincronizarProvinciasAsync(force: false));
                    
                    return provinciasLocal;
                }

                // 2. Si no hay datos locales, intentar desde API
                _logger.LogWarning("No hay provincias en BD local, intentando desde API Georef...");
                var provinciasApi = await ObtenerProvinciasDesdeApiAsync();
                
                if (provinciasApi.Any())
                {
                    await GuardarProvinciasEnBDAsync(provinciasApi);
                    return provinciasApi;
                }

                // 3. Fallback final: datos hardcodeados
                _logger.LogError("API Georef no disponible, usando fallback hardcodeado");
                return ObtenerProvinciasFallback();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener provincias");
                return ObtenerProvinciasFallback();
            }
        }

        /// <summary>
        /// Obtiene localidades: primero desde BD local, sincroniza si es necesario
        /// </summary>
        public async Task<List<LocalidadDto>> ObtenerLocalidadesAsync(string provinciaNombre)
        {
            try
            {
                // 1. Obtener desde BD local
                var localidadesLocal = await _context.GeorefLocalidades
                    .Include(l => l.Provincia)
                    .Where(l => l.Provincia!.Nombre == provinciaNombre)
                    .OrderBy(l => l.Nombre)
                    .Select(l => new LocalidadDto
                    {
                        Id = l.GeorefId,
                        Nombre = l.Nombre
                    })
                    .ToListAsync();

                if (localidadesLocal.Any())
                {
                    _logger.LogInformation($"Localidades de {provinciaNombre} desde BD local: {localidadesLocal.Count}");
                    
                    // Sincronizar en background si hace más de 30 días
                    _ = Task.Run(() => SincronizarLocalidadesAsync(provinciaNombre, force: false));
                    
                    return localidadesLocal;
                }

                // 2. Si no hay datos locales, intentar desde API
                _logger.LogWarning($"No hay localidades de {provinciaNombre} en BD local, intentando desde API...");
                var localidadesApi = await ObtenerLocalidadesDesdeApiAsync(provinciaNombre);
                
                if (localidadesApi.Any())
                {
                    await GuardarLocalidadesEnBDAsync(localidadesApi, provinciaNombre);
                    return localidadesApi;
                }

                // 3. Fallback: lista vacía o datos predefinidos para San Luis
                if (provinciaNombre == "San Luis")
                {
                    return ObtenerLocalidadesSanLuisFallback();
                }

                return new List<LocalidadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener localidades de {provinciaNombre}");
                
                if (provinciaNombre == "San Luis")
                {
                    return ObtenerLocalidadesSanLuisFallback();
                }
                
                return new List<LocalidadDto>();
            }
        }

        #endregion

        #region API Georef

        private async Task<List<ProvinciaDto>> ObtenerProvinciasDesdeApiAsync()
        {
            try
            {
                var url = $"{GEOREF_API_BASE}/provincias?campos=id,nombre&max=24";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Georef provincias retornó: {response.StatusCode}");
                    return new List<ProvinciaDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeorefApiResponse<ProvinciaGeorefApi>>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Provincias == null)
                {
                    _logger.LogWarning("API Georef retornó respuesta vacía para provincias");
                    return new List<ProvinciaDto>();
                }

                return result.Provincias.Select(p => new ProvinciaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre
                }).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de conexión con API Georef (provincias)");
                return new List<ProvinciaDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar respuesta de API Georef (provincias)");
                return new List<ProvinciaDto>();
            }
        }

        private async Task<List<LocalidadDto>> ObtenerLocalidadesDesdeApiAsync(string provinciaNombre)
        {
            try
            {
                var url = $"{GEOREF_API_BASE}/localidades?provincia={Uri.EscapeDataString(provinciaNombre)}&campos=id,nombre&max=5000";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Georef localidades retornó: {response.StatusCode}");
                    return new List<LocalidadDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeorefApiResponse<LocalidadGeorefApi>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Localidades == null)
                {
                    _logger.LogWarning($"API Georef retornó respuesta vacía para localidades de {provinciaNombre}");
                    return new List<LocalidadDto>();
                }

                return result.Localidades.Select(l => new LocalidadDto
                {
                    Id = l.Id,
                    Nombre = l.Nombre
                }).ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error de conexión con API Georef (localidades de {provinciaNombre})");
                return new List<LocalidadDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar respuesta de API Georef (localidades de {provinciaNombre})");
                return new List<LocalidadDto>();
            }
        }

        #endregion

        #region Guardar en BD

        private async Task GuardarProvinciasEnBDAsync(List<ProvinciaDto> provincias)
        {
            try
            {
                var now = DateTime.Now;
                
                foreach (var prov in provincias)
                {
                    var existe = await _context.GeorefProvincias
                        .FirstOrDefaultAsync(p => p.GeorefId == prov.Id);

                    if (existe != null)
                    {
                        existe.Nombre = prov.Nombre;
                        existe.FechaActualizacion = now;
                    }
                    else
                    {
                        _context.GeorefProvincias.Add(new GeorefProvincia
                        {
                            GeorefId = prov.Id,
                            Nombre = prov.Nombre,
                            FechaCreacion = now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                
                _context.GeorefSyncLogs.Add(new GeorefSyncLog
                {
                    TipoSincronizacion = "provincias",
                    Exitosa = true,
                    CantidadRegistros = provincias.Count
                });
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Provincias sincronizadas en BD: {provincias.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar provincias en BD");
                
                _context.GeorefSyncLogs.Add(new GeorefSyncLog
                {
                    TipoSincronizacion = "provincias",
                    Exitosa = false,
                    MensajeError = ex.Message
                });
                await _context.SaveChangesAsync();
            }
        }

        private async Task GuardarLocalidadesEnBDAsync(List<LocalidadDto> localidades, string provinciaNombre)
        {
            try
            {
                var provincia = await _context.GeorefProvincias
                    .FirstOrDefaultAsync(p => p.Nombre == provinciaNombre);

                if (provincia == null)
                {
                    _logger.LogWarning($"Provincia {provinciaNombre} no existe en BD, no se pueden guardar localidades");
                    return;
                }

                var now = DateTime.Now;
                
                foreach (var loc in localidades)
                {
                    var existe = await _context.GeorefLocalidades
                        .FirstOrDefaultAsync(l => l.GeorefId == loc.Id);

                    if (existe != null)
                    {
                        existe.Nombre = loc.Nombre;
                        existe.FechaActualizacion = now;
                    }
                    else
                    {
                        _context.GeorefLocalidades.Add(new GeorefLocalidad
                        {
                            GeorefId = loc.Id,
                            Nombre = loc.Nombre,
                            ProvinciaId = provincia.Id,
                            FechaCreacion = now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                
                _context.GeorefSyncLogs.Add(new GeorefSyncLog
                {
                    TipoSincronizacion = $"localidades_{provinciaNombre}",
                    Exitosa = true,
                    CantidadRegistros = localidades.Count
                });
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Localidades de {provinciaNombre} sincronizadas en BD: {localidades.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al guardar localidades de {provinciaNombre} en BD");
                
                _context.GeorefSyncLogs.Add(new GeorefSyncLog
                {
                    TipoSincronizacion = $"localidades_{provinciaNombre}",
                    Exitosa = false,
                    MensajeError = ex.Message
                });
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Sincronización Manual

        /// <summary>
        /// Sincroniza provincias desde API (puede forzarse o solo si pasó tiempo)
        /// </summary>
        public async Task<bool> SincronizarProvinciasAsync(bool force = false)
        {
            try
            {
                if (!force)
                {
                    // Verificar si ya se sincronizó hace menos de 30 días
                    var ultimaSync = await _context.GeorefSyncLogs
                        .Where(s => s.TipoSincronizacion == "provincias" && s.Exitosa)
                        .OrderByDescending(s => s.FechaSincronizacion)
                        .FirstOrDefaultAsync();

                    if (ultimaSync != null && (DateTime.Now - ultimaSync.FechaSincronizacion).TotalDays < 30)
                    {
                        _logger.LogInformation("Provincias ya sincronizadas recientemente, omitiendo");
                        return true;
                    }
                }

                var provincias = await ObtenerProvinciasDesdeApiAsync();
                
                if (provincias.Any())
                {
                    await GuardarProvinciasEnBDAsync(provincias);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en sincronización de provincias");
                return false;
            }
        }

        /// <summary>
        /// Sincroniza localidades de una provincia desde API
        /// </summary>
        public async Task<bool> SincronizarLocalidadesAsync(string provinciaNombre, bool force = false)
        {
            try
            {
                if (!force)
                {
                    var clave = $"localidades_{provinciaNombre}";
                    var ultimaSync = await _context.GeorefSyncLogs
                        .Where(s => s.TipoSincronizacion == clave && s.Exitosa)
                        .OrderByDescending(s => s.FechaSincronizacion)
                        .FirstOrDefaultAsync();

                    if (ultimaSync != null && (DateTime.Now - ultimaSync.FechaSincronizacion).TotalDays < 30)
                    {
                        _logger.LogInformation($"Localidades de {provinciaNombre} ya sincronizadas recientemente");
                        return true;
                    }
                }

                var localidades = await ObtenerLocalidadesDesdeApiAsync(provinciaNombre);
                
                if (localidades.Any())
                {
                    await GuardarLocalidadesEnBDAsync(localidades, provinciaNombre);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en sincronización de localidades de {provinciaNombre}");
                return false;
            }
        }

        #endregion

        #region Fallback Hardcodeado

        private List<ProvinciaDto> ObtenerProvinciasFallback()
        {
            return new List<ProvinciaDto>
            {
                new ProvinciaDto { Id = "02", Nombre = "Buenos Aires" },
                new ProvinciaDto { Id = "06", Nombre = "Ciudad Autónoma de Buenos Aires" },
                new ProvinciaDto { Id = "14", Nombre = "Córdoba" },
                new ProvinciaDto { Id = "50", Nombre = "Mendoza" },
                new ProvinciaDto { Id = "74", Nombre = "San Luis" },
                new ProvinciaDto { Id = "82", Nombre = "Santa Fe" }
            };
        }

        private List<LocalidadDto> ObtenerLocalidadesSanLuisFallback()
        {
            return new List<LocalidadDto>
            {
                new LocalidadDto { Id = "740007", Nombre = "San Luis" },
                new LocalidadDto { Id = "740014", Nombre = "Villa Mercedes" },
                new LocalidadDto { Id = "740021", Nombre = "Merlo" },
                new LocalidadDto { Id = "740028", Nombre = "La Punta" }
            };
        }

        #endregion

        #region DTOs y Modelos de API

        public class ProvinciaDto
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        public class LocalidadDto
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        private class GeorefApiResponse<T>
        {
            public List<T>? Provincias { get; set; }
            public List<T>? Localidades { get; set; }
        }

        private class ProvinciaGeorefApi
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        private class LocalidadGeorefApi
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }

        #endregion
    }
}
