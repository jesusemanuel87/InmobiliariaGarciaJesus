using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public class ContratoStateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContratoStateService> _logger;

        public ContratoStateService(IServiceProvider serviceProvider, ILogger<ContratoStateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateContractStates();
                    _logger.LogInformation("Contract states updated at {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract states");
                }

                // Ejecutar cada hora
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task UpdateContractStates()
        {
            using var scope = _serviceProvider.CreateScope();
            var contratoRepository = scope.ServiceProvider.GetRequiredService<IRepository<Contrato>>() as ContratoRepository;
            
            if (contratoRepository == null) return;

            var today = DateTime.Today;
            var contratos = await contratoRepository.GetAllAsync();

            foreach (var contrato in contratos)
            {
                var newState = DetermineContractState(contrato, today);
                
                if (newState != contrato.Estado)
                {
                    contrato.Estado = newState;
                    await contratoRepository.UpdateAsync(contrato);
                    
                    _logger.LogInformation("Contract {ContratoId} state changed to {NewState}", 
                        contrato.Id, newState);
                }
            }
        }

        private EstadoContrato DetermineContractState(Contrato contrato, DateTime today)
        {
            // Si el contrato está cancelado, mantener ese estado
            if (contrato.Estado == EstadoContrato.Cancelado)
            {
                return EstadoContrato.Cancelado;
            }

            // Si la fecha de fin ya pasó, marcar como finalizado
            if (contrato.FechaFin < today)
            {
                return EstadoContrato.Finalizado;
            }

            // Si la fecha de inicio ya llegó, marcar como activo
            if (contrato.FechaInicio <= today && contrato.FechaFin >= today)
            {
                return EstadoContrato.Activo;
            }

            // Si la fecha de inicio es futura, marcar como reservado
            if (contrato.FechaInicio > today)
            {
                return EstadoContrato.Reservado;
            }

            return contrato.Estado;
        }
    }
}
