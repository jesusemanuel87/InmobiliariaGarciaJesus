using InmobiliariaGarciaJesus.Data;
using Microsoft.EntityFrameworkCore;

namespace InmobiliariaGarciaJesus.Services
{
    /// <summary>
    /// Servicio en background para sincronizar provincias y localidades automáticamente
    /// - Al iniciar la aplicación (si no hay datos o pasaron >30 días)
    /// - Cada 30 días en background
    /// </summary>
    public class GeorefBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GeorefBackgroundService> _logger;
        private const int INTERVALO_DIAS = 30; // Sincronizar cada 30 días
        private readonly TimeSpan _intervaloEjecucion = TimeSpan.FromDays(INTERVALO_DIAS);

        public GeorefBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<GeorefBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("=== GeorefBackgroundService iniciado ===");

            // Esperar 10 segundos para que el servidor termine de iniciar
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            // Sincronización inicial al levantar el servidor
            await SincronizarInicialAsync(stoppingToken);

            // Loop infinito para sincronización periódica
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation($"Próxima sincronización de Georef en {INTERVALO_DIAS} días");
                    
                    // Esperar 30 días antes de la próxima sincronización
                    await Task.Delay(_intervaloEjecucion, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await SincronizarProvinciasYLocalidadesAsync(stoppingToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("GeorefBackgroundService detenido");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en loop de sincronización de Georef");
                    // Esperar 1 hora antes de reintentar en caso de error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Sincronización inicial al levantar el servidor
        /// </summary>
        private async Task SincronizarInicialAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InmobiliariaDbContext>();

                // Verificar si hay provincias en la BD
                var hayProvincias = await context.GeorefProvincias.AnyAsync(stoppingToken);

                if (!hayProvincias)
                {
                    _logger.LogWarning("⚠️ No hay provincias en BD, ejecutando sincronización inicial obligatoria...");
                    await SincronizarProvinciasYLocalidadesAsync(stoppingToken);
                    return;
                }

                // Verificar cuándo fue la última sincronización exitosa
                var ultimaSync = await context.GeorefSyncLogs
                    .Where(s => s.TipoSincronizacion == "provincias" && s.Exitosa)
                    .OrderByDescending(s => s.FechaSincronizacion)
                    .FirstOrDefaultAsync(stoppingToken);

                if (ultimaSync == null)
                {
                    _logger.LogWarning("⚠️ No hay registro de sincronización previa, ejecutando sincronización inicial...");
                    await SincronizarProvinciasYLocalidadesAsync(stoppingToken);
                    return;
                }

                var diasDesdeUltimaSync = (DateTime.Now - ultimaSync.FechaSincronizacion).TotalDays;
                _logger.LogInformation($"📅 Última sincronización: {ultimaSync.FechaSincronizacion:dd/MM/yyyy HH:mm} ({diasDesdeUltimaSync:F0} días atrás)");

                if (diasDesdeUltimaSync >= INTERVALO_DIAS)
                {
                    _logger.LogInformation($"⏰ Han pasado {diasDesdeUltimaSync:F0} días, ejecutando sincronización...");
                    await SincronizarProvinciasYLocalidadesAsync(stoppingToken);
                }
                else
                {
                    _logger.LogInformation($"✅ Datos de Georef actualizados (última sync hace {diasDesdeUltimaSync:F0} días)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en sincronización inicial de Georef");
            }
        }

        /// <summary>
        /// Sincroniza provincias y las 5 localidades más importantes de cada provincia
        /// </summary>
        private async Task SincronizarProvinciasYLocalidadesAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("🔄 Iniciando sincronización de provincias y localidades desde API Georef...");

                using var scope = _serviceProvider.CreateScope();
                var georefService = scope.ServiceProvider.GetRequiredService<GeorefService>();

                // 1. Sincronizar provincias
                var provinciasSincronizadas = await georefService.SincronizarProvinciasAsync(force: true);
                
                if (provinciasSincronizadas)
                {
                    _logger.LogInformation("✅ Provincias sincronizadas correctamente");

                    // 2. Sincronizar localidades de provincias principales
                    var provinciasImportantes = new[] 
                    { 
                        "San Luis",           // Tu provincia
                        "Buenos Aires",
                        "Ciudad Autónoma de Buenos Aires",
                        "Córdoba",
                        "Santa Fe",
                        "Mendoza"
                    };

                    foreach (var provincia in provinciasImportantes)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var localidadesSincronizadas = await georefService.SincronizarLocalidadesAsync(provincia, force: true);
                            
                            if (localidadesSincronizadas)
                            {
                                _logger.LogInformation($"✅ Localidades de {provincia} sincronizadas");
                            }
                            else
                            {
                                _logger.LogWarning($"⚠️ No se pudieron sincronizar localidades de {provincia}");
                            }

                            // Pequeña pausa para no saturar la API
                            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ Error al sincronizar localidades de {provincia}");
                        }
                    }

                    _logger.LogInformation("✅ Sincronización de Georef completada exitosamente");
                }
                else
                {
                    _logger.LogWarning("⚠️ No se pudieron sincronizar las provincias desde API Georef");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico en sincronización de Georef");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("=== GeorefBackgroundService detenido ===");
            return base.StopAsync(cancellationToken);
        }
    }
}
