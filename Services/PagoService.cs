using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public interface IPagoService
    {
        Task GenerarPlanPagosAsync(int contratoId);
        Task<IEnumerable<Pago>> GetPagosByContratoAsync(int contratoId);
        Task ActualizarEstadosPagosAsync();
        Task CalcularInteresesYMultasAsync(int pagoId);
        Task<decimal> CalcularInteresesPorRetrasoAsync(DateTime fechaVencimiento, decimal importe);
        Task<decimal> CalcularMultasPorRetrasoAsync(DateTime fechaVencimiento, decimal importe);
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
                var fechaVencimiento = fechaPago.AddMonths(i);
                // Establecer vencimiento el día 10 del mes
                fechaVencimiento = new DateTime(fechaVencimiento.Year, fechaVencimiento.Month, 10);

                var pago = new Pago
                {
                    Numero = numeroPago,
                    ContratoId = contratoId,
                    Importe = contrato.Precio,
                    FechaPago = null, // Se establecerá cuando se registre el pago
                    FechaVencimiento = fechaVencimiento,
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
            return allPagos.Where(p => p.ContratoId == contratoId).OrderBy(p => p.FechaVencimiento);
        }

        public async Task ActualizarEstadosPagosAsync()
        {
            var pagos = await _pagoRepository.GetAllAsync();
            // Incluir pagos pendientes sin fecha de pago registrada
            var pagosPendientes = pagos.Where(p => 
                (p.Estado == EstadoPago.Pendiente || p.Estado == EstadoPago.Vencido) && 
                p.FechaPago == null);

            var hoy = DateTime.Today;
            bool cambiosRealizados = false;
            
            foreach (var pago in pagosPendientes)
            {
                // Si pasó la fecha de vencimiento, asegurar que esté marcado como Vencido
                if (hoy > pago.FechaVencimiento && pago.Estado != EstadoPago.Vencido)
                {
                    pago.Estado = EstadoPago.Vencido;
                    await _pagoRepository.UpdateAsync(pago);
                    cambiosRealizados = true;
                }
                
                // Calcular intereses para pagos vencidos sin intereses calculados
                if (pago.Estado == EstadoPago.Vencido && pago.Intereses == 0)
                {
                    await CalcularInteresesYMultasAsync(pago.Id);
                    cambiosRealizados = true;
                }
            }
            
            // Log para debugging
            if (cambiosRealizados)
            {
                Console.WriteLine($"[{DateTime.Now}] ActualizarEstadosPagosAsync: Se realizaron cambios en los pagos");
            }
        }

        public async Task CalcularInteresesYMultasAsync(int pagoId)
        {
            var pago = await _pagoRepository.GetByIdAsync(pagoId);
            if (pago == null) return;

            var fechaPago = pago.FechaPago ?? DateTime.Today;
            
            // Calcular intereses y multas basados en los días de retraso
            pago.Intereses = await CalcularInteresesPorRetrasoAsync(pago.FechaVencimiento, pago.Importe);
            pago.Multas = await CalcularMultasPorRetrasoAsync(pago.FechaVencimiento, pago.Importe);

            await _pagoRepository.UpdateAsync(pago);
        }

        public async Task<decimal> CalcularInteresesPorRetrasoAsync(DateTime fechaVencimiento, decimal importe)
        {
            var hoy = DateTime.Today;
            var diasRetraso = (hoy - fechaVencimiento).Days;

            if (diasRetraso <= 0) return 0; // No hay retraso

            var configuraciones = await _configuracionRepository.GetAllAsync();
            decimal porcentajeInteres = 0;

            // Calcular meses completos de retraso
            var mesesRetraso = ((hoy.Year - fechaVencimiento.Year) * 12) + hoy.Month - fechaVencimiento.Month;
            
            // Del 1 al 10: Sin interés
            if (diasRetraso <= 10)
            {
                return 0; // No aplica interés
            }
            // Del 11 al 20: INTERES_VENCIMIENTO_10_20 (configurado: 10%)
            else if (diasRetraso <= 20)
            {
                var config = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_10_20");
                if (config != null && decimal.TryParse(config.Valor, out var valor))
                    porcentajeInteres = valor;
            }
            // Si pasó 1 mes completo o más: INTERES_VENCIMIENTO_MENSUAL (configurado: 30%)
            else if (mesesRetraso >= 1)
            {
                var config = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_MENSUAL");
                if (config != null && decimal.TryParse(config.Valor, out var valor))
                    porcentajeInteres = valor;
            }
            // Del 21 hasta fin de mes (mismo mes): INTERES_VENCIMIENTO_20_PLUS (configurado: 20%)
            else
            {
                var config = configuraciones.FirstOrDefault(c => c.Clave == "INTERES_VENCIMIENTO_20_PLUS");
                if (config != null && decimal.TryParse(config.Valor, out var valor))
                    porcentajeInteres = valor;
            }

            return importe * (porcentajeInteres / 100);
        }

        public Task<decimal> CalcularMultasPorRetrasoAsync(DateTime fechaVencimiento, decimal importe)
        {
            // Las multas por retraso en pagos no se aplican automáticamente
            // Las multas se aplican solo en casos de terminación temprana/tardía de contratos
            return Task.FromResult(0m);
        }

        public async Task<decimal> CalcularMultaTerminacionAsync(int contratoId, DateTime fechaTerminacion, bool esTerminacionTemprana)
        {
            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null) return 0;

            var configuraciones = await _configuracionRepository.GetAllAsync();
            decimal mesesMulta = 0;

            if (esTerminacionTemprana)
            {
                // Multa por terminación temprana
                var config = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TEMPRANA");
                if (config != null && decimal.TryParse(config.Valor, out var valor))
                    mesesMulta = valor;
            }
            else
            {
                // Multa por terminación tardía
                var config = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TARDIA");
                if (config != null && decimal.TryParse(config.Valor, out var valor))
                    mesesMulta = valor;
            }

            return contrato.Precio * mesesMulta;
        }
    }
}
