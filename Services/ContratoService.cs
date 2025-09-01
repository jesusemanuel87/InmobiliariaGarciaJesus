using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public interface IContratoService
    {
        Task<IEnumerable<Contrato>> GetAllAsync();
        Task<Contrato?> GetByIdAsync(int id);
        Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateContratoAsync(Contrato contrato, bool isEdit = false);
        Task<Contrato> CreateContratoAsync(Contrato contrato);
        Task<Contrato> UpdateContratoAsync(Contrato contrato);
        Task<bool> DeleteContratoAsync(int id);
        Task<List<(DateTime FechaInicio, DateTime FechaFin)>> GetUnavailableDatesAsync(int inmuebleId);
        Task<DateTime?> GetNextAvailableDateAsync(int inmuebleId);
        Task<ContratoFinalizacionViewModel> CalcularFinalizacionAsync(int contratoId, DateTime fechaFinalizacion);
        Task<Contrato> FinalizarContratoAsync(ContratoFinalizacionViewModel modelo);
        Task<Contrato> CancelarContratoAsync(int contratoId, string motivo);
    }

    public class ContratoService : IContratoService
    {
        private readonly IRepository<Contrato> _contratoRepository;
        private readonly IRepository<Pago> _pagoRepository;
        private readonly IRepository<Configuracion> _configuracionRepository;

        public ContratoService(IRepository<Contrato> contratoRepository,
                              IRepository<Pago> pagoRepository,
                              IRepository<Configuracion> configuracionRepository)
        {
            _contratoRepository = contratoRepository;
            _pagoRepository = pagoRepository;
            _configuracionRepository = configuracionRepository;
        }

        public async Task<IEnumerable<Contrato>> GetAllAsync()
        {
            return await _contratoRepository.GetAllAsync();
        }

        public async Task<Contrato?> GetByIdAsync(int id)
        {
            return await _contratoRepository.GetByIdAsync(id);
        }

        public async Task<(bool IsValid, Dictionary<string, string> Errors)> ValidateContratoAsync(Contrato contrato, bool isEdit = false)
        {
            var errors = new Dictionary<string, string>();

            // Obtener contrato original si es edición
            Contrato? contratoOriginal = null;
            if (isEdit && contrato.Id > 0)
            {
                contratoOriginal = await _contratoRepository.GetByIdAsync(contrato.Id);
            }

            // Validar que la fecha de fin sea posterior a la fecha de inicio
            if (contrato.FechaFin <= contrato.FechaInicio)
            {
                errors.Add("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
            }

            // No permitir modificar fecha de inicio si el contrato está activo
            if (isEdit && contratoOriginal != null && contratoOriginal.Estado == EstadoContrato.Activo)
            {
                if (contrato.FechaInicio != contratoOriginal.FechaInicio)
                {
                    errors.Add("FechaInicio", "No se puede modificar la fecha de inicio de un contrato activo");
                }
            }

            // Validar cancelación
            if (contrato.Estado == EstadoContrato.Cancelado)
            {
                if (string.IsNullOrWhiteSpace(contrato.MotivoCancelacion))
                {
                    errors.Add("MotivoCancelacion", "El motivo de cancelación es obligatorio");
                }
            }

            // Validar que las fechas no sean anteriores a hoy para contratos activos o reservados (solo para nuevos contratos)
            if (!isEdit && contrato.FechaInicio < DateTime.Today && 
                (contrato.Estado == EstadoContrato.Activo || contrato.Estado == EstadoContrato.Reservado))
            {
                errors.Add("FechaInicio", "La fecha de inicio no puede ser anterior a hoy para contratos activos o reservados");
            }

            if (!isEdit && contrato.FechaFin < DateTime.Today && 
                (contrato.Estado == EstadoContrato.Activo || contrato.Estado == EstadoContrato.Reservado))
            {
                errors.Add("FechaFin", "La fecha de fin no puede ser anterior a hoy para contratos activos o reservados");
            }

            // Validar que no haya contratos superpuestos (solo si no es cancelación)
            if (errors.Count == 0 && contrato.Estado != EstadoContrato.Cancelado)
            {
                var contratoRepository = (ContratoRepository)_contratoRepository;
                
                // Para edición, excluir el contrato actual de la validación
                int? excludeId = isEdit ? contrato.Id : null;
                
                var hasOverlap = await contratoRepository.HasOverlappingContractAsync(
                    contrato.InmuebleId, contrato.FechaInicio, contrato.FechaFin, excludeId);

                if (hasOverlap)
                {
                    errors.Add("", "Ya existe un contrato activo para este inmueble en las fechas seleccionadas");
                }
            }

            return (errors.Count == 0, errors);
        }

        public async Task<Contrato> CreateContratoAsync(Contrato contrato)
        {
            contrato.FechaCreacion = DateTime.Now;
            
            // Determinar el estado inicial basado en la fecha de inicio
            if (contrato.FechaInicio > DateTime.Today)
            {
                contrato.Estado = EstadoContrato.Reservado;
            }
            else
            {
                contrato.Estado = EstadoContrato.Activo;
            }

            var contratoId = await _contratoRepository.CreateAsync(contrato);
            contrato.Id = contratoId;
            return contrato;
        }

        public async Task<Contrato> UpdateContratoAsync(Contrato contrato)
        {
            // Si se está cancelando el contrato, actualizar fecha de fin a hoy
            if (contrato.Estado == EstadoContrato.Cancelado)
            {
                contrato.FechaFin = DateTime.Today;
            }
            
            await _contratoRepository.UpdateAsync(contrato);
            return contrato;
        }

        public async Task<bool> DeleteContratoAsync(int id)
        {
            return await _contratoRepository.DeleteAsync(id);
        }

        public async Task<List<(DateTime FechaInicio, DateTime FechaFin)>> GetUnavailableDatesAsync(int inmuebleId)
        {
            var contratoRepository = (ContratoRepository)_contratoRepository;
            return await contratoRepository.GetUnavailableDatesAsync(inmuebleId);
        }

        public async Task<DateTime?> GetNextAvailableDateAsync(int inmuebleId)
        {
            var contratoRepository = (ContratoRepository)_contratoRepository;
            return await contratoRepository.GetNextAvailableDateAsync(inmuebleId);
        }

        public async Task<ContratoFinalizacionViewModel> CalcularFinalizacionAsync(int contratoId, DateTime fechaFinalizacion)
        {
            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null) throw new ArgumentException("Contrato no encontrado");

            // Obtener configuración de multas
            var configuraciones = await _configuracionRepository.GetAllAsync();
            var multaMenorMitad = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_MENOR_MITAD");
            var multaMayorMitad = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_MAYOR_MITAD");
        
            if (multaMenorMitad == null || multaMayorMitad == null)
            {
                throw new InvalidOperationException("No se encontró configuración de multas");
            }

            // Calcular porcentaje de tiempo cumplido
            var tiempoTotal = (contrato.FechaFin - contrato.FechaInicio).Days;
            var tiempoCumplido = (fechaFinalizacion - contrato.FechaInicio).Days;
            var porcentajeCumplido = (double)tiempoCumplido / tiempoTotal;

            // Aplicar multa según porcentaje cumplido
            decimal porcentajeMulta;
            if (porcentajeCumplido < 0.5) // Menos del 50%
            {
                porcentajeMulta = decimal.Parse(multaMenorMitad.Valor);
            }
            else // 50% o más
            {
                porcentajeMulta = decimal.Parse(multaMayorMitad.Valor);
            }

            var modelo = new ContratoFinalizacionViewModel
            {
                ContratoId = contratoId,
                Contrato = contrato,
                FechaFinalizacion = fechaFinalizacion
            };

            // Calcular meses totales y cumplidos
            var mesesTotales = ((contrato.FechaFin.Year - contrato.FechaInicio.Year) * 12) + 
                              contrato.FechaFin.Month - contrato.FechaInicio.Month;
            var mesesCumplidos = ((fechaFinalizacion.Year - contrato.FechaInicio.Year) * 12) + 
                                fechaFinalizacion.Month - contrato.FechaInicio.Month;

            modelo.MesesTotales = mesesTotales;
            modelo.MesesCumplidos = mesesCumplidos;
            modelo.EsFinalizacionTemprana = mesesCumplidos < mesesTotales;

            // Obtener pagos atrasados
            var allPagos = await _pagoRepository.GetAllAsync();
            var pagosContrato = allPagos.Where(p => p.ContratoId == contratoId).ToList();
            modelo.PagosAtrasados = pagosContrato.Where(p => p.Estado == EstadoPago.Pendiente || p.Estado == EstadoPago.Vencido).ToList();

            modelo.MesesAdeudados = modelo.PagosAtrasados.Count;
            modelo.ImporteAdeudado = modelo.PagosAtrasados.Sum(p => p.Importe);

            // Calcular multa si es finalización temprana
            if (modelo.EsFinalizacionTemprana)
            {
                // Usar el porcentaje de multa ya calculado anteriormente
                modelo.MultaCalculada = contrato.Precio * (porcentajeMulta / 100m);
                
                // Agregar multa adicional por pagos atrasados si los hay
                if (modelo.MesesAdeudados > 0)
                {
                    modelo.MultaCalculada += modelo.ImporteAdeudado * 0.1m; // 10% adicional por pagos atrasados
                }
            }

            return modelo;
        }

        public async Task<Contrato> FinalizarContratoAsync(ContratoFinalizacionViewModel modelo)
        {
            var contrato = await _contratoRepository.GetByIdAsync(modelo.ContratoId);
            if (contrato == null) throw new ArgumentException("Contrato no encontrado");

            contrato.Estado = EstadoContrato.Finalizado;
            contrato.FechaFinalizacionReal = modelo.FechaFinalizacion;
            contrato.MultaFinalizacion = modelo.MultaCalculada;
            contrato.MesesAdeudados = modelo.MesesAdeudados;
            contrato.ImporteAdeudado = modelo.ImporteAdeudado;
            contrato.MotivoCancelacion = modelo.Motivo;

            await _contratoRepository.UpdateAsync(contrato);

            // Crear pago de multa si corresponde
            if (modelo.MultaCalculada.HasValue && modelo.MultaCalculada > 0)
            {
                var pagoMulta = new Pago
                {
                    Numero = 999, // Número especial para multas
                    ContratoId = contrato.Id,
                    Importe = modelo.MultaCalculada.Value,
                    FechaPago = modelo.FechaFinalizacion,
                    Estado = EstadoPago.Pendiente,
                    FechaCreacion = DateTime.Now,
                    Observaciones = "Multa por finalización temprana del contrato"
                };
                await _pagoRepository.CreateAsync(pagoMulta);
            }

            return contrato;
        }

        public async Task<Contrato> CancelarContratoAsync(int contratoId, string motivo)
        {
            var contrato = await _contratoRepository.GetByIdAsync(contratoId);
            if (contrato == null) throw new ArgumentException("Contrato no encontrado");

            contrato.Estado = EstadoContrato.Cancelado;
            contrato.MotivoCancelacion = motivo;
            contrato.FechaFinalizacionReal = DateTime.Today;

            await _contratoRepository.UpdateAsync(contrato);
            return contrato;
        }
    }
}
