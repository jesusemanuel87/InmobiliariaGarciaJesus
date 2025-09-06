using InmobiliariaGarciaJesus.Services;

namespace InmobiliariaGarciaJesus.Services
{
    public class PagoStateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PagoStateService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Verificar cada hora

        public PagoStateService(IServiceProvider serviceProvider, ILogger<PagoStateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de actualización de estados de pagos iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ActualizarEstadosPagos();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Servicio detenido normalmente
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar estados de pagos");
                    // Esperar un poco antes de reintentar en caso de error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Servicio de actualización de estados de pagos detenido");
        }

        private async Task ActualizarEstadosPagos()
        {
            using var scope = _serviceProvider.CreateScope();
            var pagoService = scope.ServiceProvider.GetRequiredService<IPagoService>();

            try
            {
                await pagoService.ActualizarEstadosPagosAsync();
                _logger.LogDebug("Estados de pagos actualizados correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estados de pagos");
                throw;
            }
        }
    }
}
