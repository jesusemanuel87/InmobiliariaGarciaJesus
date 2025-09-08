using MySql.Data.MySqlClient;
using InmobiliariaGarciaJesus.Models;
using System.Data;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class EmpleadoRepository : IRepository<Empleado>
    {
        private readonly string _connectionString;

        public EmpleadoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            var empleados = new List<Empleado>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, 
                                         Observaciones, FechaCreacion, FechaModificacion, Estado 
                                         FROM Empleados WHERE Estado = 1 ORDER BY Apellido, Nombre";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                empleados.Add(MapFromReader(reader));
            }
            
            return empleados;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync(Func<Empleado, bool> filter)
        {
            var empleados = await GetAllAsync();
            return empleados.Where(filter);
        }

        public async Task<Empleado?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, 
                                         Observaciones, FechaCreacion, FechaModificacion, Estado 
                                         FROM Empleados WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Empleado empleado)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"INSERT INTO Empleados (Nombre, Apellido, Dni, Telefono, Email, Rol, 
                                                        FechaIngreso, Observaciones, FechaCreacion, Estado) 
                                 VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Email, @Rol, 
                                         @FechaIngreso, @Observaciones, @FechaCreacion, @Estado);
                                 SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", empleado.Nombre);
            command.Parameters.AddWithValue("@Apellido", empleado.Apellido);
            command.Parameters.AddWithValue("@Dni", empleado.Dni);
            command.Parameters.AddWithValue("@Telefono", empleado.Telefono);
            command.Parameters.AddWithValue("@Email", empleado.Email);
            command.Parameters.AddWithValue("@Rol", (int)empleado.Rol);
            command.Parameters.AddWithValue("@FechaIngreso", empleado.FechaIngreso);
            command.Parameters.AddWithValue("@Observaciones", empleado.Observaciones ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaCreacion", empleado.FechaCreacion);
            command.Parameters.AddWithValue("@Estado", empleado.Estado);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Empleado empleado)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"UPDATE Empleados SET 
                                 Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, 
                                 Telefono = @Telefono, Email = @Email, Rol = @Rol, 
                                 FechaIngreso = @FechaIngreso, Observaciones = @Observaciones, 
                                 FechaModificacion = @FechaModificacion, Estado = @Estado
                                 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", empleado.Id);
            command.Parameters.AddWithValue("@Nombre", empleado.Nombre);
            command.Parameters.AddWithValue("@Apellido", empleado.Apellido);
            command.Parameters.AddWithValue("@Dni", empleado.Dni);
            command.Parameters.AddWithValue("@Telefono", empleado.Telefono);
            command.Parameters.AddWithValue("@Email", empleado.Email);
            command.Parameters.AddWithValue("@Rol", (int)empleado.Rol);
            command.Parameters.AddWithValue("@FechaIngreso", empleado.FechaIngreso);
            command.Parameters.AddWithValue("@Observaciones", empleado.Observaciones ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);
            command.Parameters.AddWithValue("@Estado", empleado.Estado);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"UPDATE Empleados SET Estado = 0, FechaModificacion = @FechaModificacion WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string query = "SELECT COUNT(*) FROM Empleados WHERE Email = @Email AND Estado = 1";
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

        public async Task<bool> DniExistsAsync(string dni, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string query = "SELECT COUNT(*) FROM Empleados WHERE Dni = @Dni AND Estado = 1";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Dni", dni);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<Empleado?> GetByEmailAsync(string email)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, 
                                         Observaciones, FechaCreacion, FechaModificacion, Estado 
                                         FROM Empleados WHERE Email = @Email AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<Empleado?> GetByDniAsync(string dni)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Rol, FechaIngreso, 
                                         Observaciones, FechaCreacion, FechaModificacion, Estado 
                                         FROM Empleados WHERE Dni = @Dni AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Dni", dni);
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            const string query = "SELECT COUNT(*) FROM Empleados WHERE Id = @Id AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        private static Empleado MapFromReader(IDataReader reader)
        {
            return new Empleado
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                Dni = reader.GetString(reader.GetOrdinal("Dni")),
                Telefono = reader.GetString(reader.GetOrdinal("Telefono")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Rol = (RolEmpleado)reader.GetInt32(reader.GetOrdinal("Rol")),
                FechaIngreso = reader.GetDateTime(reader.GetOrdinal("FechaIngreso")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                Estado = reader.GetBoolean(reader.GetOrdinal("Estado"))
            };
        }
    }
}
