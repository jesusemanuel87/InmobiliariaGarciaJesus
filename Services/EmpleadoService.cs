using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;

namespace InmobiliariaGarciaJesus.Services
{
    public class EmpleadoService
    {
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly ILogger<EmpleadoService> _logger;

        public EmpleadoService(EmpleadoRepository empleadoRepository, ILogger<EmpleadoService> logger)
        {
            _empleadoRepository = empleadoRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Empleado>> GetAllEmpleadosAsync()
        {
            try
            {
                return await _empleadoRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los empleados");
                throw;
            }
        }

        public async Task<Empleado?> GetEmpleadoByIdAsync(int id)
        {
            try
            {
                return await _empleadoRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado por ID: {Id}", id);
                throw;
            }
        }

        public async Task<Empleado?> GetEmpleadoByEmailAsync(string email)
        {
            try
            {
                return await _empleadoRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado por email: {Email}", email);
                throw;
            }
        }

        public async Task<Empleado?> GetEmpleadoByDniAsync(string dni)
        {
            try
            {
                return await _empleadoRepository.GetByDniAsync(dni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleado por DNI: {Dni}", dni);
                throw;
            }
        }

        public async Task<(bool Success, string Message, int? EmpleadoId)> CreateEmpleadoAsync(Empleado empleado)
        {
            try
            {
                // Validar que no exista el email
                if (await _empleadoRepository.EmailExistsAsync(empleado.Email))
                {
                    return (false, "Ya existe un empleado con este email", null);
                }

                // Validar que no exista el DNI
                if (await _empleadoRepository.DniExistsAsync(empleado.Dni))
                {
                    return (false, "Ya existe un empleado con este DNI", null);
                }

                var empleadoId = await _empleadoRepository.CreateAsync(empleado);
                
                _logger.LogInformation("Empleado creado exitosamente: {EmpleadoId}", empleadoId);
                return (true, "Empleado creado exitosamente", empleadoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empleado");
                return (false, "Error interno del servidor", null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateEmpleadoAsync(Empleado empleado)
        {
            try
            {
                // Verificar que el empleado existe
                var existingEmpleado = await _empleadoRepository.GetByIdAsync(empleado.Id);
                if (existingEmpleado == null)
                {
                    return (false, "El empleado no existe");
                }

                // Validar que no exista el email en otro empleado
                if (await _empleadoRepository.EmailExistsAsync(empleado.Email, empleado.Id))
                {
                    return (false, "Ya existe otro empleado con este email");
                }

                // Validar que no exista el DNI en otro empleado
                if (await _empleadoRepository.DniExistsAsync(empleado.Dni, empleado.Id))
                {
                    return (false, "Ya existe otro empleado con este DNI");
                }

                var success = await _empleadoRepository.UpdateAsync(empleado);
                
                if (success)
                {
                    _logger.LogInformation("Empleado actualizado exitosamente: {EmpleadoId}", empleado.Id);
                    return (true, "Empleado actualizado exitosamente");
                }
                
                return (false, "No se pudo actualizar el empleado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empleado: {EmpleadoId}", empleado.Id);
                return (false, "Error interno del servidor");
            }
        }

        public async Task<(bool Success, string Message)> DeleteEmpleadoAsync(int id)
        {
            try
            {
                // Verificar que el empleado existe
                if (!await _empleadoRepository.ExistsAsync(id))
                {
                    return (false, "El empleado no existe");
                }

                // TODO: Verificar que no tenga usuarios asociados activos
                // Esta validación se implementará cuando se integre con UsuarioService

                var success = await _empleadoRepository.DeleteAsync(id);
                
                if (success)
                {
                    _logger.LogInformation("Empleado eliminado (lógicamente) exitosamente: {EmpleadoId}", id);
                    return (true, "Empleado eliminado exitosamente");
                }
                
                return (false, "No se pudo eliminar el empleado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empleado: {EmpleadoId}", id);
                return (false, "Error interno del servidor");
            }
        }

        public async Task<bool> EmpleadoExistsAsync(int id)
        {
            try
            {
                return await _empleadoRepository.ExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de empleado: {Id}", id);
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            try
            {
                return await _empleadoRepository.EmailExistsAsync(email, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> DniExistsAsync(string dni, int? excludeId = null)
        {
            try
            {
                return await _empleadoRepository.DniExistsAsync(dni, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de DNI: {Dni}", dni);
                return false;
            }
        }

        public async Task<IEnumerable<Empleado>> GetEmpleadosByRolAsync(RolEmpleado rol)
        {
            try
            {
                var empleados = await _empleadoRepository.GetAllAsync();
                return empleados.Where(e => e.Rol == rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empleados por rol: {Rol}", rol);
                throw;
            }
        }

        public async Task<IEnumerable<Empleado>> GetAdministradoresAsync()
        {
            return await GetEmpleadosByRolAsync(RolEmpleado.Administrador);
        }

        public async Task<IEnumerable<Empleado>> GetEmpleadosRegularesAsync()
        {
            return await GetEmpleadosByRolAsync(RolEmpleado.Empleado);
        }
    }
}
