using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Repositories;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Services
{
    public class DatabaseSeederService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(IConfiguration configuration, ILogger<DatabaseSeederService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
            _logger = logger;
        }

        public async Task SeedDefaultAdminUserAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar si ya existe el usuario admin
                var checkUserQuery = "SELECT COUNT(*) FROM Usuarios WHERE Email = @Email AND Estado = 1";
                using var checkCommand = new MySqlCommand(checkUserQuery, connection);
                checkCommand.Parameters.AddWithValue("@Email", "admin@inmobiliaria.com");
                
                var userExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (userExists)
                {
                    _logger.LogInformation("Usuario administrador ya existe en la base de datos");
                    return;
                }

                // Crear empleado administrador
                var empleadoQuery = @"INSERT IGNORE INTO Empleados 
                    (Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, Observaciones, FechaCreacion, Estado) 
                    VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Email, @Rol, @FechaIngreso, @Observaciones, @FechaCreacion, @Estado)";

                using var empleadoCommand = new MySqlCommand(empleadoQuery, connection);
                empleadoCommand.Parameters.AddWithValue("@Nombre", "Admin");
                empleadoCommand.Parameters.AddWithValue("@Apellido", "Sistema");
                empleadoCommand.Parameters.AddWithValue("@Dni", "00000000");
                empleadoCommand.Parameters.AddWithValue("@Telefono", "0000000000");
                empleadoCommand.Parameters.AddWithValue("@Email", "admin@inmobiliaria.com");
                empleadoCommand.Parameters.AddWithValue("@Rol", (int)RolEmpleado.Administrador);
                empleadoCommand.Parameters.AddWithValue("@FechaIngreso", DateTime.Now);
                empleadoCommand.Parameters.AddWithValue("@Observaciones", "Usuario administrador del sistema");
                empleadoCommand.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                empleadoCommand.Parameters.AddWithValue("@Estado", true);

                await empleadoCommand.ExecuteNonQueryAsync();

                // Obtener el ID del empleado
                var getEmpleadoIdQuery = "SELECT Id FROM Empleados WHERE Email = @Email";
                using var getIdCommand = new MySqlCommand(getEmpleadoIdQuery, connection);
                getIdCommand.Parameters.AddWithValue("@Email", "admin@inmobiliaria.com");
                
                var empleadoIdResult = await getIdCommand.ExecuteScalarAsync();
                var empleadoId = Convert.ToInt32(empleadoIdResult);

                // Crear usuario administrador con hash correcto
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                
                var usuarioQuery = @"INSERT INTO Usuarios 
                    (NombreUsuario, Email, ClaveHash, Rol, EmpleadoId, FechaCreacion, Estado) 
                    VALUES (@NombreUsuario, @Email, @ClaveHash, @Rol, @EmpleadoId, @FechaCreacion, @Estado)";

                using var usuarioCommand = new MySqlCommand(usuarioQuery, connection);
                usuarioCommand.Parameters.AddWithValue("@NombreUsuario", "admin");
                usuarioCommand.Parameters.AddWithValue("@Email", "admin@inmobiliaria.com");
                usuarioCommand.Parameters.AddWithValue("@ClaveHash", passwordHash);
                usuarioCommand.Parameters.AddWithValue("@Rol", (int)RolUsuario.Administrador);
                usuarioCommand.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                usuarioCommand.Parameters.AddWithValue("@FechaCreacion", DateTime.Now);
                usuarioCommand.Parameters.AddWithValue("@Estado", true);

                await usuarioCommand.ExecuteNonQueryAsync();

                _logger.LogInformation("Usuario administrador creado exitosamente: admin@inmobiliaria.com / admin123");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario administrador por defecto");
                throw;
            }
        }

        public async Task<bool> VerifyAdminUserAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT u.Id, u.NombreUsuario, u.Email, u.ClaveHash, u.Estado 
                             FROM Usuarios u 
                             WHERE u.Email = @Email AND u.Estado = 1";
                
                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", "admin@inmobiliaria.com");
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var claveHash = reader.GetString(reader.GetOrdinal("ClaveHash"));
                    var isValidHash = BCrypt.Net.BCrypt.Verify("admin123", claveHash);
                    
                    _logger.LogInformation("Usuario admin encontrado. Hash válido: {IsValid}", isValidHash);
                    return isValidHash;
                }
                
                _logger.LogWarning("Usuario admin no encontrado en la base de datos");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar usuario administrador");
                return false;
            }
        }
    }
}
