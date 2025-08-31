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
    }

    public class ContratoService : IContratoService
    {
        private readonly IRepository<Contrato> _contratoRepository;

        public ContratoService(IRepository<Contrato> contratoRepository)
        {
            _contratoRepository = contratoRepository;
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
    }
}
