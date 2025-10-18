using MySql.Data.MySqlClient;
using InmobiliariaGarciaJesus.Models;
using System.Data;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class UsuarioRepository : IRepository<Usuario>
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            var usuarios = new List<Usuario>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.FotoPerfil, u.Rol, 
                         u.FechaCreacion, u.UltimoAcceso, u.Estado, u.RequiereCambioClave, u.EmpleadoId, u.PropietarioId, u.InquilinoId,
                         e.Nombre as EmpleadoNombre, e.Apellido as EmpleadoApellido, e.Rol as EmpleadoRol,
                         p.Nombre as PropietarioNombre, p.Apellido as PropietarioApellido,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido
                         FROM Usuarios u
                         LEFT JOIN Empleados e ON u.EmpleadoId = e.Id
                         LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
                         LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
                         ORDER BY u.NombreUsuario";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                usuarios.Add(MapFromReaderWithRelations(reader));
            }
            
            return usuarios;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync(Func<Usuario, bool> filter)
        {
            var usuarios = await GetAllAsync();
            return usuarios.Where(filter);
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.FotoPerfil, u.Rol, 
                         u.FechaCreacion, u.UltimoAcceso, u.Estado, u.RequiereCambioClave, u.EmpleadoId, u.PropietarioId, u.InquilinoId,
                         e.Nombre as EmpleadoNombre, e.Apellido as EmpleadoApellido, e.Dni as EmpleadoDni, 
                         e.Telefono as EmpleadoTelefono, e.Email as EmpleadoEmail, e.Rol as EmpleadoRol, e.FechaIngreso,
                         p.Nombre as PropietarioNombre, p.Apellido as PropietarioApellido, p.Dni as PropietarioDni,
                         p.Telefono as PropietarioTelefono, p.Email as PropietarioEmail,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido, i.Dni as InquilinoDni,
                         i.Telefono as InquilinoTelefono, i.Email as InquilinoEmail
                         FROM Usuarios u
                         LEFT JOIN Empleados e ON u.EmpleadoId = e.Id
                         LEFT JOIN Propietarios p ON u.PropietarioId = p.Id
                         LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id
                         WHERE u.Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReaderComplete(reader);
            }
            
            return null;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.FotoPerfil, u.Rol, 
                         u.FechaCreacion, u.UltimoAcceso, u.Estado, u.RequiereCambioClave, u.EmpleadoId, u.PropietarioId, u.InquilinoId,
                         e.Nombre as EmpleadoNombre, e.Apellido as EmpleadoApellido, e.Dni as EmpleadoDni, 
                         e.Telefono as EmpleadoTelefono, e.Email as EmpleadoEmail, e.Rol as EmpleadoRol, e.FechaIngreso,
                         p.Nombre as PropietarioNombre, p.Apellido as PropietarioApellido, p.Dni as PropietarioDni,
                         p.Telefono as PropietarioTelefono, p.Email as PropietarioEmail,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido, i.Dni as InquilinoDni,
                         i.Telefono as InquilinoTelefono, i.Email as InquilinoEmail
                         FROM Usuarios u
                         LEFT JOIN Empleados e ON u.EmpleadoId = e.Id AND e.Estado = 1
                         LEFT JOIN Propietarios p ON u.PropietarioId = p.Id AND p.Estado = 1
                         LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id AND i.Estado = 1
                         WHERE u.Email = @Email AND u.Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReaderComplete(reader);
            }
            
            return null;
        }

        public async Task<Usuario?> GetByUsernameAsync(string username)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.FotoPerfil, u.Rol, 
                         u.FechaCreacion, u.UltimoAcceso, u.Estado, u.RequiereCambioClave, u.EmpleadoId, u.PropietarioId, u.InquilinoId,
                         e.Nombre as EmpleadoNombre, e.Apellido as EmpleadoApellido, e.Dni as EmpleadoDni, 
                         e.Telefono as EmpleadoTelefono, e.Email as EmpleadoEmail, e.Rol as EmpleadoRol, e.FechaIngreso,
                         p.Nombre as PropietarioNombre, p.Apellido as PropietarioApellido, p.Dni as PropietarioDni,
                         p.Telefono as PropietarioTelefono, p.Email as PropietarioEmail,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido, i.Dni as InquilinoDni,
                         i.Telefono as InquilinoTelefono, i.Email as InquilinoEmail
                         FROM Usuarios u
                         LEFT JOIN Empleados e ON u.EmpleadoId = e.Id AND e.Estado = 1
                         LEFT JOIN Propietarios p ON u.PropietarioId = p.Id AND p.Estado = 1
                         LEFT JOIN Inquilinos i ON u.InquilinoId = i.Id AND i.Estado = 1
                         WHERE u.NombreUsuario = @Username AND u.Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReaderComplete(reader);
            }
            
            return null;
        }

        public async Task<List<Usuario>> GetUsuariosByPersonaAsync(int personaId, RolUsuario tipoPersona)
        {
            var usuarios = new List<Usuario>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string whereClause = tipoPersona switch
            {
                RolUsuario.Propietario => "u.PropietarioId = @PersonaId",
                RolUsuario.Inquilino => "u.InquilinoId = @PersonaId",
                RolUsuario.Empleado or RolUsuario.Administrador => "u.EmpleadoId = @PersonaId",
                _ => "1 = 0" // No match
            };
            
            var query = $@"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.FotoPerfil, u.Rol, 
                          u.FechaCreacion, u.UltimoAcceso, u.Estado, u.RequiereCambioClave, u.EmpleadoId, u.PropietarioId, u.InquilinoId
                          FROM Usuarios u
                          WHERE {whereClause} AND u.Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PersonaId", personaId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                usuarios.Add(MapFromReader(reader));
            }
            
            return usuarios;
        }

        public async Task<int> CreateAsync(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Usuarios (NombreUsuario, Email, ClaveHash, FotoPerfil, Rol, 
                         EmpleadoId, PropietarioId, InquilinoId, Estado, RequiereCambioClave) 
                         VALUES (@NombreUsuario, @Email, @ClaveHash, @FotoPerfil, @Rol, 
                         @EmpleadoId, @PropietarioId, @InquilinoId, @Estado, @RequiereCambioClave);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            AddParameters(command, usuario);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE Usuarios SET 
                         NombreUsuario = @NombreUsuario, Email = @Email, ClaveHash = @ClaveHash,
                         FotoPerfil = @FotoPerfil, Rol = @Rol, EmpleadoId = @EmpleadoId,
                         PropietarioId = @PropietarioId, InquilinoId = @InquilinoId, Estado = @Estado, RequiereCambioClave = @RequiereCambioClave
                         WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            AddParameters(command, usuario);
            command.Parameters.AddWithValue("@Id", usuario.Id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastAccessAsync(int usuarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "UPDATE Usuarios SET UltimoAcceso = CURRENT_TIMESTAMP WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", usuarioId);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Eliminación lógica
            var query = "UPDATE Usuarios SET Estado = 0 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM Usuarios WHERE Id = @Id AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email AND Estado = 1";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> EmailExistsWithRoleAsync(string email, RolUsuario rol, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email AND Rol = @Rol";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Rol", (int)rol);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "SELECT COUNT(*) FROM Usuarios WHERE NombreUsuario = @Username AND Estado = 1";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        private static Usuario MapFromReader(IDataReader reader)
        {
            return new Usuario
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                ClaveHash = reader.GetString(reader.GetOrdinal("ClaveHash")),
                FotoPerfil = reader.IsDBNull(reader.GetOrdinal("FotoPerfil")) ? null : reader.GetString(reader.GetOrdinal("FotoPerfil")),
                Rol = (RolUsuario)reader.GetInt32(reader.GetOrdinal("Rol")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("UltimoAcceso")) ? null : reader.GetDateTime(reader.GetOrdinal("UltimoAcceso")),
                Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),
                RequiereCambioClave = reader.GetBoolean(reader.GetOrdinal("RequiereCambioClave")),
                EmpleadoId = reader.IsDBNull(reader.GetOrdinal("EmpleadoId")) ? null : reader.GetInt32(reader.GetOrdinal("EmpleadoId")),
                PropietarioId = reader.IsDBNull(reader.GetOrdinal("PropietarioId")) ? null : reader.GetInt32(reader.GetOrdinal("PropietarioId")),
                InquilinoId = reader.IsDBNull(reader.GetOrdinal("InquilinoId")) ? null : reader.GetInt32(reader.GetOrdinal("InquilinoId"))
            };
        }

        private static Usuario MapFromReaderWithRelations(IDataReader reader)
        {
            var usuario = MapFromReader(reader);
            
            // Cargar relaciones básicas
            if (usuario.EmpleadoId.HasValue && !reader.IsDBNull(reader.GetOrdinal("EmpleadoNombre")))
            {
                usuario.Empleado = new Empleado
                {
                    Id = usuario.EmpleadoId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("EmpleadoNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("EmpleadoApellido")),
                    Rol = (RolEmpleado)reader.GetInt32(reader.GetOrdinal("EmpleadoRol"))
                };
            }
            
            if (usuario.PropietarioId.HasValue && !reader.IsDBNull(reader.GetOrdinal("PropietarioNombre")))
            {
                usuario.Propietario = new Propietario
                {
                    Id = usuario.PropietarioId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("PropietarioNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("PropietarioApellido"))
                };
            }
            
            if (usuario.InquilinoId.HasValue && !reader.IsDBNull(reader.GetOrdinal("InquilinoNombre")))
            {
                usuario.Inquilino = new Inquilino
                {
                    Id = usuario.InquilinoId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("InquilinoNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("InquilinoApellido"))
                };
            }
            
            return usuario;
        }

        private static Usuario MapFromReaderComplete(IDataReader reader)
        {
            var usuario = MapFromReader(reader);
            
            // Cargar relaciones completas
            if (usuario.EmpleadoId.HasValue && !reader.IsDBNull(reader.GetOrdinal("EmpleadoNombre")))
            {
                usuario.Empleado = new Empleado
                {
                    Id = usuario.EmpleadoId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("EmpleadoNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("EmpleadoApellido")),
                    Dni = reader.GetString(reader.GetOrdinal("EmpleadoDni")),
                    Telefono = reader.GetString(reader.GetOrdinal("EmpleadoTelefono")),
                    Email = reader.GetString(reader.GetOrdinal("EmpleadoEmail")),
                    Rol = (RolEmpleado)reader.GetInt32(reader.GetOrdinal("EmpleadoRol")),
                    FechaIngreso = reader.GetDateTime(reader.GetOrdinal("FechaIngreso"))
                };
            }
            
            if (usuario.PropietarioId.HasValue && !reader.IsDBNull(reader.GetOrdinal("PropietarioNombre")))
            {
                usuario.Propietario = new Propietario
                {
                    Id = usuario.PropietarioId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("PropietarioNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("PropietarioApellido")),
                    Dni = reader.GetString(reader.GetOrdinal("PropietarioDni")),
                    Telefono = reader.GetString(reader.GetOrdinal("PropietarioTelefono")),
                    Email = reader.GetString(reader.GetOrdinal("PropietarioEmail"))
                };
            }
            
            if (usuario.InquilinoId.HasValue && !reader.IsDBNull(reader.GetOrdinal("InquilinoNombre")))
            {
                usuario.Inquilino = new Inquilino
                {
                    Id = usuario.InquilinoId.Value,
                    Nombre = reader.GetString(reader.GetOrdinal("InquilinoNombre")),
                    Apellido = reader.GetString(reader.GetOrdinal("InquilinoApellido")),
                    Dni = reader.GetString(reader.GetOrdinal("InquilinoDni")),
                    Telefono = reader.GetString(reader.GetOrdinal("InquilinoTelefono")),
                    Email = reader.GetString(reader.GetOrdinal("InquilinoEmail"))
                };
            }
            
            return usuario;
        }

        private static void AddParameters(MySqlCommand command, Usuario usuario)
        {
            command.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@ClaveHash", usuario.ClaveHash);
            command.Parameters.AddWithValue("@FotoPerfil", usuario.FotoPerfil ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Rol", (int)usuario.Rol);
            command.Parameters.AddWithValue("@EmpleadoId", usuario.EmpleadoId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PropietarioId", usuario.PropietarioId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@InquilinoId", usuario.InquilinoId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Estado", usuario.Estado);
            command.Parameters.AddWithValue("@RequiereCambioClave", usuario.RequiereCambioClave);
        }
    }
}
