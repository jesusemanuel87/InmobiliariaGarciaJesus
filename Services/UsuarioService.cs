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
                
                if (usuario == null)
                {
                    return (false, "Credenciales inválidas", null);
                }

                if (!VerifyPassword(password, usuario.ClaveHash))
                {
                    return (false, "Credenciales inválidas", null);
                }

                // Validar que el usuario esté activo DESPUÉS de validar contraseña
                if (!usuario.Estado)
                {
                    return (false, "Su cuenta está pendiente de validación por un administrador. Por favor, espere a que su cuenta sea activada.", null);
                }

                // Validar que si es empleado/administrador, el empleado esté activo
                if ((usuario.Rol == RolUsuario.Empleado || usuario.Rol == RolUsuario.Administrador) && 
                    (usuario.Empleado == null || !usuario.Empleado.Estado))
                {
                    return (false, "Credenciales inválidas", null);
                }

                // Validar que si es propietario, el propietario esté activo
                if (usuario.Rol == RolUsuario.Propietario && 
                    (usuario.Propietario == null || !usuario.Propietario.Estado))
                {
                    return (false, "Credenciales inválidas", null);
                }

                // Validar que si es inquilino, el inquilino esté activo
                if (usuario.Rol == RolUsuario.Inquilino && 
                    (usuario.Inquilino == null || !usuario.Inquilino.Estado))
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
                // Validar que no exista el email CON EL MISMO ROL (permite multi-rol con mismo email)
                if (await _usuarioRepository.EmailExistsWithRoleAsync(model.Email, model.TipoUsuario))
                {
                    return (false, $"Ya existe un usuario registrado como {model.TipoUsuario} con este email. Si olvidó su contraseña, use la opción de recuperación.", null);
                }

                // Validar que no exista el username (debe ser único globalmente)
                if (await _usuarioRepository.UsernameExistsAsync(model.NombreUsuario))
                {
                    return (false, "Ya existe un usuario con este nombre de usuario", null);
                }

                int? personaId = null;
                
                // Obtener o crear la persona según el tipo de usuario
                try
                {
                    switch (model.TipoUsuario)
                    {
                        case RolUsuario.Propietario:
                            personaId = await GetOrCreatePropietarioAsync(model);
                            break;
                        case RolUsuario.Inquilino:
                            personaId = await GetOrCreateInquilinoAsync(model);
                            break;
                        case RolUsuario.Empleado:
                        case RolUsuario.Administrador:
                            return (false, "Los empleados deben ser creados por un administrador", null);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Error específico de validación (ej: DNI ya tiene usuario)
                    return (false, ex.Message, null);
                }

                if (!personaId.HasValue)
                {
                    return (false, "Error al procesar la información personal", null);
                }

                // Crear el usuario con estado INACTIVO por defecto
                var usuario = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Email = model.Email,
                    ClaveHash = HashPassword(model.Password),
                    Rol = model.TipoUsuario,
                    Estado = false  // INACTIVO - Requiere validación de administrador
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
                
                _logger.LogInformation("Usuario registrado con estado INACTIVO: {Email}, Rol: {Rol}, DNI: {Dni}", 
                    model.Email, model.TipoUsuario, model.Dni);
                return (true, "Registro exitoso. Su cuenta será activada por un administrador", usuarioId);
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
                _logger.LogInformation("GetRolesDisponiblesAsync: Buscando roles para usuario {UsuarioId} con email {Email}", 
                    usuario.Id, usuario.Email);

                // Buscar TODOS los usuarios con el mismo email para detectar multi-rol
                var todosLosUsuarios = await _usuarioRepository.GetAllAsync();
                _logger.LogInformation("GetRolesDisponiblesAsync: Total usuarios en BD: {Count}", todosLosUsuarios.Count());
                
                var usuariosConMismoEmail = todosLosUsuarios
                    .Where(u => u.Email.Equals(usuario.Email, StringComparison.OrdinalIgnoreCase) && u.Estado)
                    .ToList();

                _logger.LogInformation("GetRolesDisponiblesAsync: Usuarios con email {Email} y activos: {Count}", 
                    usuario.Email, usuariosConMismoEmail.Count);

                // Agregar roles únicos de todos los usuarios con este email
                foreach (var u in usuariosConMismoEmail)
                {
                    _logger.LogInformation("GetRolesDisponiblesAsync: Usuario {Id} tiene rol {Rol}", u.Id, u.Rol);
                    
                    if (!roles.Contains(u.Rol))
                    {
                        // Para Administrador, verificar que sea empleado con rol de admin
                        if (u.Rol == RolUsuario.Administrador && u.EmpleadoId.HasValue)
                        {
                            var empleado = await _empleadoRepository.GetByIdAsync(u.EmpleadoId.Value);
                            if (empleado?.Rol == RolEmpleado.Administrador)
                            {
                                roles.Add(RolUsuario.Administrador);
                                _logger.LogInformation("GetRolesDisponiblesAsync: Agregado rol Administrador");
                            }
                        }
                        else
                        {
                            roles.Add(u.Rol);
                            _logger.LogInformation("GetRolesDisponiblesAsync: Agregado rol {Rol}", u.Rol);
                        }
                    }
                }

                _logger.LogInformation("GetRolesDisponiblesAsync: Roles disponibles finales: {Count} - [{Roles}]", 
                    roles.Count, string.Join(", ", roles));

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

        private async Task<int?> GetOrCreatePropietarioAsync(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Apellido) || 
                string.IsNullOrEmpty(model.Dni) || string.IsNullOrEmpty(model.Telefono))
            {
                return null;
            }

            try
            {
                // Buscar si ya existe un propietario con este DNI
                var propietarioExistente = await _propietarioRepository.GetByDniAsync(model.Dni);
                
                if (propietarioExistente != null)
                {
                    // Si existe, verificar si ya tiene un usuario asociado como propietario
                    var usuariosExistentes = await _usuarioRepository.GetUsuariosByPersonaAsync(
                        propietarioExistente.Id, RolUsuario.Propietario);
                    
                    if (usuariosExistentes.Any())
                    {
                        throw new InvalidOperationException(
                            $"Ya existe un usuario registrado como Propietario con el DNI {model.Dni}. " +
                            "Si olvidó su contraseña, use la opción de recuperación.");
                    }
                    
                    // Si NO tiene usuario, reutilizar el ID existente
                    _logger.LogInformation("Reutilizando propietario existente con DNI {Dni} (Id: {Id})", 
                        model.Dni, propietarioExistente.Id);
                    return propietarioExistente.Id;
                }

                // Si NO existe, crear nuevo propietario con estado INACTIVO
                var propietario = new Propietario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dni = model.Dni,
                    Telefono = model.Telefono,
                    Email = model.Email,
                    Estado = false  // INACTIVO - Requiere validación de administrador
                };

                var propietarioId = await _propietarioRepository.CreateAsync(propietario);
                _logger.LogInformation("Nuevo propietario creado con estado INACTIVO: DNI {Dni} (Id: {Id})", 
                    model.Dni, propietarioId);
                return propietarioId;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de validación
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener o crear propietario con DNI: {Dni}", model.Dni);
                throw;
            }
        }

        private async Task<int?> GetOrCreateInquilinoAsync(RegisterViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nombre) || string.IsNullOrEmpty(model.Apellido) || 
                string.IsNullOrEmpty(model.Dni) || string.IsNullOrEmpty(model.Telefono))
            {
                return null;
            }

            try
            {
                // Buscar si ya existe un inquilino con este DNI
                var inquilinoExistente = await _inquilinoRepository.GetByDniAsync(model.Dni);
                
                if (inquilinoExistente != null)
                {
                    // Si existe, verificar si ya tiene un usuario asociado como inquilino
                    var usuariosExistentes = await _usuarioRepository.GetUsuariosByPersonaAsync(
                        inquilinoExistente.Id, RolUsuario.Inquilino);
                    
                    if (usuariosExistentes.Any())
                    {
                        throw new InvalidOperationException(
                            $"Ya existe un usuario registrado como Inquilino con el DNI {model.Dni}. " +
                            "Si olvidó su contraseña, use la opción de recuperación.");
                    }
                    
                    // Si NO tiene usuario, reutilizar el ID existente
                    _logger.LogInformation("Reutilizando inquilino existente con DNI {Dni} (Id: {Id})", 
                        model.Dni, inquilinoExistente.Id);
                    return inquilinoExistente.Id;
                }

                // Si NO existe, crear nuevo inquilino con estado INACTIVO
                var inquilino = new Inquilino
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dni = model.Dni,
                    Telefono = model.Telefono,
                    Email = model.Email,
                    Estado = false  // INACTIVO - Requiere validación de administrador
                };

                var inquilinoId = await _inquilinoRepository.CreateAsync(inquilino);
                _logger.LogInformation("Nuevo inquilino creado con estado INACTIVO: DNI {Dni} (Id: {Id})", 
                    model.Dni, inquilinoId);
                return inquilinoId;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de validación
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener o crear inquilino con DNI: {Dni}", model.Dni);
                throw;
            }
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        // Método para registrar usuario multi-rol (copia datos de rol existente)
        public async Task<(bool Success, string Message, int? UsuarioId)> RegisterMultiRoleUsuarioAsync(RegisterViewModel model, string existingPassword)
        {
            try
            {
                // Validar email y obtener usuario existente
                var (exists, existingUser, errorMessage) = await ValidateExistingEmailForMultiRoleAsync(model.Email, model.TipoUsuario, existingPassword);
                
                if (!exists || existingUser == null)
                {
                    return (false, errorMessage ?? "No se pudo validar la cuenta existente", null);
                }

                // Validar username único
                if (await _usuarioRepository.UsernameExistsAsync(model.NombreUsuario))
                {
                    return (false, "Ya existe un usuario con este nombre de usuario", null);
                }

                int? personaId = null;
                string dni = "";
                string nombre = "";
                string apellido = "";
                string telefono = "";

                // Obtener datos del usuario existente
                if (existingUser.Propietario != null)
                {
                    dni = existingUser.Propietario.Dni;
                    nombre = existingUser.Propietario.Nombre;
                    apellido = existingUser.Propietario.Apellido;
                    telefono = existingUser.Propietario.Telefono ?? "";
                }
                else if (existingUser.Inquilino != null)
                {
                    dni = existingUser.Inquilino.Dni;
                    nombre = existingUser.Inquilino.Nombre;
                    apellido = existingUser.Inquilino.Apellido;
                    telefono = existingUser.Inquilino.Telefono ?? "";
                }
                else if (existingUser.Empleado != null)
                {
                    dni = existingUser.Empleado.Dni;
                    nombre = existingUser.Empleado.Nombre;
                    apellido = existingUser.Empleado.Apellido;
                    telefono = existingUser.Empleado.Telefono;
                }

                // Obtener o crear persona según el rol
                try
                {
                    if (model.TipoUsuario == RolUsuario.Propietario)
                    {
                        // Buscar si ya existe un propietario con este DNI
                        var propietarioExistente = await _propietarioRepository.GetByDniAsync(dni);
                        
                        if (propietarioExistente != null)
                        {
                            // Verificar si ya tiene un usuario asociado
                            var usuariosExistentes = await _usuarioRepository.GetUsuariosByPersonaAsync(
                                propietarioExistente.Id, RolUsuario.Propietario);
                            
                            if (usuariosExistentes.Any())
                            {
                                return (false, $"Ya existe un usuario Propietario con el DNI {dni}", null);
                            }
                            
                            // Reutilizar el ID existente
                            _logger.LogInformation("Reutilizando propietario existente con DNI {Dni} (Id: {Id})", 
                                dni, propietarioExistente.Id);
                            personaId = propietarioExistente.Id;
                        }
                        else
                        {
                            // Crear nuevo propietario
                            var propietario = new Propietario
                            {
                                Nombre = nombre,
                                Apellido = apellido,
                                Dni = dni,
                                Telefono = telefono,
                                Email = existingUser.Email,
                                Estado = false // Inactivo - requiere validación
                            };
                            personaId = await _propietarioRepository.CreateAsync(propietario);
                            _logger.LogInformation("Nuevo propietario creado con DNI {Dni} (Id: {Id})", dni, personaId);
                        }
                    }
                    else if (model.TipoUsuario == RolUsuario.Inquilino)
                    {
                        // Buscar si ya existe un inquilino con este DNI
                        var inquilinoExistente = await _inquilinoRepository.GetByDniAsync(dni);
                        
                        if (inquilinoExistente != null)
                        {
                            // Verificar si ya tiene un usuario asociado
                            var usuariosExistentes = await _usuarioRepository.GetUsuariosByPersonaAsync(
                                inquilinoExistente.Id, RolUsuario.Inquilino);
                            
                            if (usuariosExistentes.Any())
                            {
                                return (false, $"Ya existe un usuario Inquilino con el DNI {dni}", null);
                            }
                            
                            // Reutilizar el ID existente
                            _logger.LogInformation("Reutilizando inquilino existente con DNI {Dni} (Id: {Id})", 
                                dni, inquilinoExistente.Id);
                            personaId = inquilinoExistente.Id;
                        }
                        else
                        {
                            // Crear nuevo inquilino
                            var inquilino = new Inquilino
                            {
                                Nombre = nombre,
                                Apellido = apellido,
                                Dni = dni,
                                Telefono = telefono,
                                Email = existingUser.Email,
                                Estado = false // Inactivo - requiere validación
                            };
                            personaId = await _inquilinoRepository.CreateAsync(inquilino);
                            _logger.LogInformation("Nuevo inquilino creado con DNI {Dni} (Id: {Id})", dni, personaId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al obtener o crear persona para DNI: {Dni}", dni);
                    return (false, "Error al crear registro de persona", null);
                }

                if (!personaId.HasValue)
                {
                    return (false, "Error al crear registro de persona", null);
                }

                // Crear nuevo usuario con la misma contraseña
                var nuevoUsuario = new Usuario
                {
                    NombreUsuario = model.NombreUsuario,
                    Email = model.Email,
                    ClaveHash = existingUser.ClaveHash, // Usa la misma contraseña hasheada
                    Rol = model.TipoUsuario,
                    Estado = false // Inactivo - requiere validación
                };

                // Asignar ID de persona según rol
                if (model.TipoUsuario == RolUsuario.Propietario)
                {
                    nuevoUsuario.PropietarioId = personaId.Value;
                }
                else if (model.TipoUsuario == RolUsuario.Inquilino)
                {
                    nuevoUsuario.InquilinoId = personaId.Value;
                }

                var usuarioId = await _usuarioRepository.CreateAsync(nuevoUsuario);
                
                _logger.LogInformation("Usuario multi-rol registrado: {Email}, Nuevo rol: {Rol}", model.Email, model.TipoUsuario);
                return (true, "Registro exitoso. Su nueva cuenta será activada por un administrador", usuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario multi-rol: {Email}", model.Email);
                return (false, "Error interno del servidor", null);
            }
        }

        // Método para validar email existente y obtener datos para multi-rol
        public async Task<(bool Exists, Usuario? Usuario, string? ErrorMessage)> ValidateExistingEmailForMultiRoleAsync(string email, RolUsuario newRole, string password)
        {
            try
            {
                // Buscar usuario con este email
                var existingUser = await _usuarioRepository.GetByEmailAsync(email);
                
                if (existingUser == null)
                {
                    return (false, null, null); // Email no existe, continuar con registro normal
                }

                // Verificar que no sea el mismo rol
                if (existingUser.Rol == newRole)
                {
                    return (false, null, $"Ya tiene una cuenta registrada como {newRole}");
                }

                // Validar contraseña
                if (!VerifyPassword(password, existingUser.ClaveHash))
                {
                    return (false, null, "Contraseña incorrecta");
                }

                // Usuario validado correctamente
                _logger.LogInformation("Usuario {Email} validado para agregar rol {NewRole}", email, newRole);
                return (true, existingUser, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar email existente: {Email}", email);
                return (false, null, "Error interno del servidor");
            }
        }

        public async Task<bool> UpdateProfilePhotoAsync(int usuarioId, string fotoPerfil)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
                if (usuario == null)
                {
                    return false;
                }

                usuario.FotoPerfil = fotoPerfil;
                await _usuarioRepository.UpdateAsync(usuario);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar foto de perfil para usuario: {UsuarioId}", usuarioId);
                return false;
            }
        }
    }
}
