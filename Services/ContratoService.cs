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
        Task ProcesarPagoFinalizacionAsync(int contratoId);
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
            var multaTerminacionTemprana = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TEMPRANA");
            var multaTerminacionTardia = configuraciones.FirstOrDefault(c => c.Clave == "MULTA_TERMINACION_TARDIA");
        
            if (multaTerminacionTemprana == null || multaTerminacionTardia == null)
            {
                throw new InvalidOperationException("No se encontró configuración de multas");
            }

            // Calcular porcentaje de tiempo cumplido
            var tiempoTotal = (contrato.FechaFin - contrato.FechaInicio).Days;
            var tiempoCumplido = (fechaFinalizacion - contrato.FechaInicio).Days;
            var porcentajeCumplido = (double)tiempoCumplido / tiempoTotal;

            // Aplicar multa según porcentaje cumplido
            decimal mesesMulta;
            if (porcentajeCumplido < 0.5) // Menos del 50%
            {
                mesesMulta = decimal.Parse(multaTerminacionTemprana.Valor);
            }
            else // 50% o más
            {
                mesesMulta = decimal.Parse(multaTerminacionTardia.Valor);
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

            // Obtener pagos atrasados y del mes actual
            var allPagos = await _pagoRepository.GetAllAsync();
            var pagosContrato = allPagos.Where(p => p.ContratoId == contratoId).ToList();
            
            // Incluir pagos vencidos y el pago pendiente del mes actual
            var fechaActual = DateTime.Today;
            modelo.PagosAtrasados = pagosContrato.Where(p => 
                p.Estado == EstadoPago.Vencido || 
                (p.Estado == EstadoPago.Pendiente && p.FechaPago <= fechaActual)
            ).ToList();

            modelo.MesesAdeudados = modelo.PagosAtrasados.Count;
            modelo.ImporteAdeudado = modelo.PagosAtrasados.Sum(p => p.TotalAPagar);

            // Debug: Mostrar información del cálculo
            
            // Calcular multa si es finalización temprana
            if (modelo.EsFinalizacionTemprana)
            {
                // La multa es el precio mensual multiplicado por los meses de multa configurados
                modelo.MultaCalculada = contrato.Precio * mesesMulta;
                
                // Agregar deuda pendiente (pagos vencidos + mes actual)
                modelo.MultaCalculada += modelo.ImporteAdeudado;
                
            }
            else
            {
                // Si no es finalización temprana, solo agregar deuda pendiente
                modelo.MultaCalculada = modelo.ImporteAdeudado;
                
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

            // Eliminar pagos futuros pendientes (NO el mes actual)
            var allPagos = await _pagoRepository.GetAllAsync();
            var pagosFuturos = allPagos.Where(p => 
                p.ContratoId == contrato.Id && 
                p.Estado == EstadoPago.Pendiente && 
                p.FechaVencimiento > modelo.FechaFinalizacion.AddDays(30) // Solo eliminar pagos más de 30 días en el futuro
            ).ToList();


            foreach (var pagoFuturo in pagosFuturos)
            {
                await _pagoRepository.DeleteAsync(pagoFuturo.Id);
            }

            // Actualizar el pago del mes actual con la multa si corresponde
            
            if (modelo.MultaCalculada.HasValue && modelo.MultaCalculada > 0)
            {
                // Recargar pagos después de eliminar futuros para obtener datos actualizados
                var pagosActualizados = await _pagoRepository.GetAllAsync();
                var pagosPendientesContrato = pagosActualizados.Where(p => 
                    p.ContratoId == contrato.Id && 
                    p.Estado == EstadoPago.Pendiente
                ).ToList();
                
                
                var pagoActual = pagosPendientesContrato
                    .Where(p => p.FechaVencimiento <= modelo.FechaFinalizacion.AddDays(30))
                    .OrderByDescending(p => p.FechaVencimiento)
                    .FirstOrDefault();

                if (pagoActual != null)
                {
                    // Agregar solo la multa por terminación temprana (no incluir deuda existente)
                    var multaSoloTerminacion = modelo.MultaCalculada.Value - modelo.ImporteAdeudado;
                    
                    // Debug: Mostrar información del pago encontrado
                    
                    if (multaSoloTerminacion > 0)
                    {
                        pagoActual.Multas = multaSoloTerminacion;
                        pagoActual.Observaciones = (pagoActual.Observaciones ?? "") + " - Incluye multa por finalización temprana ($" + multaSoloTerminacion.ToString("N0") + ")";
                        var updateResult = await _pagoRepository.UpdateAsync(pagoActual);
                        
                    }
                    else
                    {
                    }
                }
                else
                {
                    
                    // Crear nuevo pago con la multa
                    var pagoMulta = new Pago
                    {
                        Numero = 999,
                        ContratoId = contrato.Id,
                        Importe = modelo.ImporteAdeudado,
                        Multas = modelo.MultaCalculada.Value - modelo.ImporteAdeudado,
                        FechaPago = modelo.FechaFinalizacion,
                        FechaVencimiento = modelo.FechaFinalizacion.AddDays(10),
                        Estado = EstadoPago.Pendiente,
                        FechaCreacion = DateTime.Now,
                        Observaciones = "Pago final con multa por finalización temprana"
                    };
                    await _pagoRepository.CreateAsync(pagoMulta);
                }
            }
            else
            {
            }

            return contrato;
        }

        public async Task ProcesarPagoFinalizacionAsync(int contratoId)
        {
            var allPagos = await _pagoRepository.GetAllAsync();
            var pagosPendientes = allPagos.Where(p => 
                p.ContratoId == contratoId && 
                p.Estado == EstadoPago.Pendiente
            ).ToList();

            foreach (var pago in pagosPendientes)
            {
                pago.Estado = EstadoPago.Pagado;
                pago.FechaPago = DateTime.Today;
                pago.MetodoPago = MetodoPago.Efectivo; // Por defecto
                await _pagoRepository.UpdateAsync(pago);
            }
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
