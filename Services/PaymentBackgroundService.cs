using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InmobiliariaGarciaJesus.Services
{
    public class PaymentBackgroundService : BackgroundService
    {
        private readonly ILogger<PaymentBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Ejecutar cada hora

        public PaymentBackgroundService(
            ILogger<PaymentBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPaymentUpdates();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancelación normal del servicio, no es un error
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el procesamiento automático de pagos");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry en 5 minutos
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancelación durante el delay, salir del loop
                        break;
                    }
                }
            }
        }

        private async Task ProcessPaymentUpdates()
        {
            using var scope = _serviceProvider.CreateScope();
            var pagoService = scope.ServiceProvider.GetRequiredService<IPagoService>() as PagoService;
            
            _logger.LogInformation("Iniciando actualización automática de estados de pagos");
            
            try
            {
                if (pagoService != null)
                {
                    await pagoService.ActualizarEstadosPagosAsync();
                    _logger.LogInformation("Actualización de estados completada exitosamente");
                }
                else
                {
                    _logger.LogError("No se pudo obtener el servicio PagoService");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estados de pagos");
                throw;
            }
        }
    }
}
