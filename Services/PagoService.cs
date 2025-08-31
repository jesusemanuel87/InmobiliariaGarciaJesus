using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public interface IPagoService
    {
        Task GenerarPlanPagosAsync(int contratoId);
        Task<IEnumerable<Pago>> GetPagosByContratoAsync(int contratoId);
    }

    public class PagoService : IPagoService
    {
        private readonly IRepository<Pago> _pagoRepository;
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IRepository<Configuracion> _configuracionRepository;

        public PagoService(IRepository<Pago> pagoRepository, 
                          IRepository<Contrato> contratoRepository,
                          IRepository<Configuracion> configuracionRepository)
        {
            _pagoRepository = pagoRepository;
            _contratoRepository = contratoRepository;
            _configuracionRepository = configuracionRepository;
        }

        public async Task GenerarPlanPagosAsync(int contratoId)
        {
            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null) return;

            // Verificar si ya existen pagos para este contrato
            var pagosExistentes = await GetPagosByContratoAsync(contratoId);
            if (pagosExistentes.Any()) return; // Ya tiene plan de pagos

            // Calcular número de cuotas basado en la duración del contrato
            var mesesContrato = ((contrato.FechaFin.Year - contrato.FechaInicio.Year) * 12) + 
                               contrato.FechaFin.Month - contrato.FechaInicio.Month;

            var fechaPago = contrato.FechaInicio;
            var numeroPago = 1;

            // Generar pagos mensuales
            for (int i = 0; i < mesesContrato; i++)
            {
                var pago = new Pago
                {
                    Numero = numeroPago,
                    ContratoId = contratoId,
                    Importe = contrato.Precio,
                    FechaPago = fechaPago.AddMonths(i),
                    Estado = EstadoPago.Pendiente,
                    FechaCreacion = DateTime.Now
                };

                await _pagoRepository.CreateAsync(pago);
                numeroPago++;
            }
        }

        public async Task<IEnumerable<Pago>> GetPagosByContratoAsync(int contratoId)
        {
            var allPagos = await _pagoRepository.GetAllAsync();
            return allPagos.Where(p => p.ContratoId == contratoId).OrderBy(p => p.FechaPago);
        }
    }
}
