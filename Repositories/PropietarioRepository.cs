using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class PropietarioRepository : IRepository<Propietario>
    {
        private readonly MySqlConnectionManager _connectionManager;

        public PropietarioRepository(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<Propietario>> GetAllAsync()
        {
            var propietarios = new List<Propietario>();
            
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "SELECT Id, Nombre, Apellido, DNI, Telefono, Email, Direccion, FechaCreacion, Estado FROM Propietarios";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                propietarios.Add(new Propietario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Apellido = reader["Apellido"].ToString() ?? string.Empty,
                    Dni = reader["DNI"].ToString() ?? string.Empty,
                    Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString() ?? string.Empty,
                    Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Estado = Convert.ToInt32(reader["Estado"]) == 1
                });
            }
            
            return propietarios;
        }

        public async Task<IEnumerable<Propietario>> GetAllAsync(Func<Propietario, bool> filter)
        {
            var allPropietarios = await GetAllAsync();
            return allPropietarios.Where(filter);
        }

        public async Task<Propietario?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Direccion, FechaCreacion, Estado 
                         FROM Propietarios WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Propietario
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Apellido = reader["Apellido"].ToString() ?? string.Empty,
                    Dni = reader["DNI"].ToString() ?? string.Empty,
                    Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString() ?? string.Empty,
                    Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Estado = Convert.ToInt32(reader["Estado"]) == 1
                };
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Propietario propietario)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Propietarios (Nombre, Apellido, Dni, Telefono, Email, Direccion, FechaCreacion, Estado) 
                         VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Email, @Direccion, @FechaCreacion, @Estado);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", propietario.Nombre);
            command.Parameters.AddWithValue("@Apellido", propietario.Apellido);
            command.Parameters.AddWithValue("@Dni", propietario.Dni);
            command.Parameters.AddWithValue("@Telefono", propietario.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", propietario.Email);
            command.Parameters.AddWithValue("@Direccion", propietario.Direccion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaCreacion", propietario.FechaCreacion);
            command.Parameters.AddWithValue("@Estado", propietario.Estado);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Propietario propietario)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"UPDATE Propietarios 
                         SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, 
                             Telefono = @Telefono, Email = @Email, Direccion = @Direccion 
                         WHERE Id = @Id AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", propietario.Id);
            command.Parameters.AddWithValue("@Nombre", propietario.Nombre);
            command.Parameters.AddWithValue("@Apellido", propietario.Apellido);
            command.Parameters.AddWithValue("@Dni", propietario.Dni);
            command.Parameters.AddWithValue("@Telefono", propietario.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", propietario.Email);
            command.Parameters.AddWithValue("@Direccion", propietario.Direccion ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "UPDATE Propietarios SET Estado = 0 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
