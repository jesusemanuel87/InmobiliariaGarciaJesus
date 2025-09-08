using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace InmobiliariaGarciaJesus.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly EmpleadoRepository _empleadoRepository;
        private readonly PropietarioRepository _propietarioRepository;
        private readonly InquilinoRepository _inquilinoRepository;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            UsuarioRepository usuarioRepository,
            EmpleadoRepository empleadoRepository,
            PropietarioRepository propietarioRepository,
            InquilinoRepository inquilinoRepository,
            ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _empleadoRepository = empleadoRepository;
            _propietarioRepository = propietarioRepository;
            _inquilinoRepository = inquilinoRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Usuario>> GetAllUsuariosAsync()
        {
            try
            {
                return await _usuarioRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            try
            {
                return await _usuarioRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                throw;
            }
        }

        public async Task<Usuario?> GetUsuarioByEmailAsync(string email)
        {
            try
            {
                return await _usuarioRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
                throw;
            }
        }

        public async Task<Usuario?> GetUsuarioByUsernameAsync(string username)
        {
            try
            {
                return await _usuarioRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por username: {Username}", username);
                throw;
            }
        }

        public async Task<(bool Success, string Message, Usuario? Usuario)> AuthenticateAsync(string email, string password)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByEmailAsync(email);
                
                if (usuario == null || !usuario.Estado)
                {
                    return (false, "Credenciales inválidas", null);
                }

                if (!VerifyPassword(password, usuario.ClaveHash))
                {
                    return (false, "Credenciales inválidas", null);
                }

                // Actualizar último acceso
                await _usuarioRepository.UpdateLastAccessAsync(usuario.Id);
                usuario.UltimoAcceso = DateTime.Now;

                _logger.LogInformation("Usuario autenticado exitosamente: {Email}", email);
                return (true, "Autenticación exitosa", usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación: {Email}", email);
                return (false, "Error interno del servidor", null);
            }
        }

        public async Task<(bool Success, string Message, int? UsuarioId)> RegisterUsuarioAsync(RegisterViewModel model)
        {
            try
            {
                // Validar que no exista el email
                if (await _usuarioRepository.EmailExistsAsync(model.Email))
                {
                    return (false, "Ya existe un usuario con este email", null);
                }

                // Validar que no exista el username
                if (await _usuarioRepository.UsernameExistsAsync(model.NombreUsuario))
                {
                    return (false, "Ya existe un usuario con este nombre de usuario", null);
                }

                int? personaId = null;
                
                // Crear la persona según el tipo de usuario
                switch (model.TipoUsuario)
                {
                    case RolUsuario.Propietario:
                        personaId = await CreatePropietarioAsync(model);
                        break;
                    case RolUsuario.Inquilino:
                        personaId = await CreateInquilinoAsync(model);
                        break;
                    case RolUsuario.Empleado:
                    case RolUsuario.Administrador:
                        return (false, "Los empleados deben ser creados por un administrador", null);
                }

                if (!personaId.HasValue)
                {
                    return (false, "Error al crear la información personal", null);
                }

                // Crear el usuario
                var usuario = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Email = model.Email,
                    ClaveHash = HashPassword(model.Password),
                    Rol = model.TipoUsuario,
                    Estado = true
                };

                // Asignar el ID de la persona según el rol
                switch (model.TipoUsuario)
                {
                    case RolUsuario.Propietario:
                        usuario.PropietarioId = personaId.Value;
                        break;
                    case RolUsuario.Inquilino:
                        usuario.InquilinoId = personaId.Value;
                        break;
                }

                var usuarioId = await _usuarioRepository.CreateAsync(usuario);
                
                _logger.LogInformation("Usuario registrado exitosamente: {Email}, Rol: {Rol}", model.Email, model.TipoUsuario);
                return (true, "Usuario registrado exitosamente", usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario: {Email}", model.Email);
                return (false, "Error interno del servidor", null);
            }
        }

        public async Task<(bool Success, string Message)> CreateEmpleadoUsuarioAsync(Empleado empleado, string nombreUsuario, string password)
        {
            try
            {
                // Validar que no exista el email
                if (await _usuarioRepository.EmailExistsAsync(empleado.Email))
                {
                    return (false, "Ya existe un usuario con este email");
                }

                // Validar que no exista el username
                if (await _usuarioRepository.UsernameExistsAsync(nombreUsuario))
                {
                    return (false, "Ya existe un usuario con este nombre de usuario");
                }

                // Crear el empleado
                var empleadoId = await _empleadoRepository.CreateAsync(empleado);

                // Crear el usuario
                var usuario = new Usuario
                {
                    NombreUsuario = nombreUsuario,
                    Email = empleado.Email,
                    ClaveHash = HashPassword(password),
                    Rol = empleado.Rol == RolEmpleado.Administrador ? RolUsuario.Administrador : RolUsuario.Empleado,
                    EmpleadoId = empleadoId,
                    Estado = true
                };

                await _usuarioRepository.CreateAsync(usuario);
                
                _logger.LogInformation("Usuario empleado creado exitosamente: {Email}, Rol: {Rol}", empleado.Email, empleado.Rol);
                return (true, "Usuario empleado creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario empleado: {Email}", empleado.Email);
                return (false, "Error interno del servidor");
            }
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int usuarioId, string currentPassword, string newPassword)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return (false, "Usuario no encontrado");
                }

                if (!VerifyPassword(currentPassword, usuario.ClaveHash))
                {
                    return (false, "La contraseña actual es incorrecta");
                }

                usuario.ClaveHash = HashPassword(newPassword);
                var success = await _usuarioRepository.UpdateAsync(usuario);

                if (success)
                {
                    _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {UsuarioId}", usuarioId);
                    return (true, "Contraseña cambiada exitosamente");
                }

                return (false, "Error al cambiar la contraseña");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar contraseña: {UsuarioId}", usuarioId);
                return (false, "Error interno del servidor");
            }
        }

        public async Task<(bool Success, string Message)> SwitchRoleAsync(int usuarioId, RolUsuario nuevoRol)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return (false, "Usuario no encontrado");
                }

                // Validar que el usuario tenga acceso a este rol
                var rolesDisponibles = await GetRolesDisponiblesAsync(usuario);
                if (!rolesDisponibles.Contains(nuevoRol))
                {
                    return (false, "No tiene permisos para cambiar a este rol");
                }

                usuario.Rol = nuevoRol;
                var success = await _usuarioRepository.UpdateAsync(usuario);

                if (success)
                {
                    _logger.LogInformation("Rol cambiado exitosamente para usuario: {UsuarioId}, Nuevo rol: {Rol}", usuarioId, nuevoRol);
                    return (true, "Rol cambiado exitosamente");
                }

                return (false, "Error al cambiar el rol");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar rol: {UsuarioId}", usuarioId);
                return (false, "Error interno del servidor");
            }
        }

        public async Task<List<RolUsuario>> GetRolesDisponiblesAsync(Usuario usuario)
        {
            var roles = new List<RolUsuario>();

            try
            {
                // Obtener todos los usuarios de esta persona para ver qué roles tiene disponibles
                if (usuario.PropietarioId.HasValue)
                {
                    var usuariosPropietario = await _usuarioRepository.GetUsuariosByPersonaAsync(usuario.PropietarioId.Value, RolUsuario.Propietario);
                    if (usuariosPropietario.Any())
                        roles.Add(RolUsuario.Propietario);
                }

                if (usuario.InquilinoId.HasValue)
                {
                    var usuariosInquilino = await _usuarioRepository.GetUsuariosByPersonaAsync(usuario.InquilinoId.Value, RolUsuario.Inquilino);
                    if (usuariosInquilino.Any())
                        roles.Add(RolUsuario.Inquilino);
                }

                if (usuario.EmpleadoId.HasValue)
                {
                    var empleado = await _empleadoRepository.GetByIdAsync(usuario.EmpleadoId.Value);
                    if (empleado != null)
                    {
                        roles.Add(RolUsuario.Empleado);
                        if (empleado.Rol == RolEmpleado.Administrador)
                        {
                            roles.Add(RolUsuario.Administrador);
                        }
                    }
                }

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles disponibles: {UsuarioId}", usuario.Id);
                return new List<RolUsuario>();
            }
        }

        public async Task<(bool Success, string Message)> DeleteUsuarioAsync(int id)
        {
            try
            {
                var success = await _usuarioRepository.DeleteAsync(id);
                
                if (success)
                {
                    _logger.LogInformation("Usuario eliminado (lógicamente) exitosamente: {UsuarioId}", id);
                    return (true, "Usuario eliminado exitosamente");
                }
                
                return (false, "No se pudo eliminar el usuario");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario: {UsuarioId}", id);
                return (false, "Error interno del servidor");
            }
        }

        private async Task<int?> CreatePropietarioAsync(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Apellido) || 
                string.IsNullOrEmpty(model.Dni) || string.IsNullOrEmpty(model.Telefono))
            {
                return null;
            }

            var propietario = new Propietario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Email = model.Email,
                Estado = true
            };

            return await _propietarioRepository.CreateAsync(propietario);
        }

        private async Task<int?> CreateInquilinoAsync(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Apellido) || 
                string.IsNullOrEmpty(model.Dni) || string.IsNullOrEmpty(model.Telefono))
            {
                return null;
            }

            var inquilino = new Inquilino
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Dni = model.Dni,
                Telefono = model.Telefono,
                Email = model.Email,
                Estado = true
            };

            return await _inquilinoRepository.CreateAsync(inquilino);
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
