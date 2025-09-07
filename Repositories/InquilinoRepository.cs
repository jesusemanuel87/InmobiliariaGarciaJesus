using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class InquilinoRepository : IRepository<Inquilino>
    {
        private readonly MySqlConnectionManager _connectionManager;

        public InquilinoRepository(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<Inquilino>> GetAllAsync()
        {
            var inquilinos = new List<Inquilino>();
            
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "SELECT Id, Nombre, Apellido, DNI, Telefono, Email, Direccion, Estado FROM Inquilinos";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                inquilinos.Add(new Inquilino
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Apellido = reader["Apellido"].ToString() ?? string.Empty,
                    Dni = reader["DNI"].ToString() ?? string.Empty,
                    Telefono = reader["Telefono"].ToString() ?? string.Empty,
                    Email = reader["Email"].ToString() ?? string.Empty,
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Estado = Convert.ToInt32(reader["Estado"]) == 1
                });
            }
            
            return inquilinos;
        }

        public async Task<IEnumerable<Inquilino>> GetAllAsync(Func<Inquilino, bool> filter)
        {
            var allInquilinos = await GetAllAsync();
            return allInquilinos.Where(filter);
        }

        public async Task<Inquilino?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, Nombre, Apellido, Dni, Telefono, Email, Direccion, FechaCreacion, Estado 
                         FROM Inquilinos WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Inquilino
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Apellido = reader["Apellido"].ToString() ?? string.Empty,
                    Dni = reader["Dni"].ToString() ?? string.Empty,
                    Telefono = reader["Telefono"] == DBNull.Value ? null : reader["Telefono"].ToString(),
                    Email = reader["Email"].ToString() ?? string.Empty,
                    Direccion = reader["Direccion"] == DBNull.Value ? null : reader["Direccion"].ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Estado = Convert.ToInt32(reader["Estado"]) == 1
                };
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Inquilino inquilino)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Inquilinos (Nombre, Apellido, Dni, Telefono, Email, Direccion, FechaCreacion, Estado) 
                         VALUES (@Nombre, @Apellido, @Dni, @Telefono, @Email, @Direccion, @FechaCreacion, @Estado);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", inquilino.Nombre);
            command.Parameters.AddWithValue("@Apellido", inquilino.Apellido);
            command.Parameters.AddWithValue("@Dni", inquilino.Dni);
            command.Parameters.AddWithValue("@Telefono", inquilino.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", inquilino.Email);
            command.Parameters.AddWithValue("@Direccion", inquilino.Direccion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@FechaCreacion", inquilino.FechaCreacion);
            command.Parameters.AddWithValue("@Estado", inquilino.Estado);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Inquilino inquilino)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"UPDATE Inquilinos 
                         SET Nombre = @Nombre, Apellido = @Apellido, Dni = @Dni, 
                             Telefono = @Telefono, Email = @Email, Direccion = @Direccion 
                         WHERE Id = @Id AND Estado = 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", inquilino.Id);
            command.Parameters.AddWithValue("@Nombre", inquilino.Nombre);
            command.Parameters.AddWithValue("@Apellido", inquilino.Apellido);
            command.Parameters.AddWithValue("@Dni", inquilino.Dni);
            command.Parameters.AddWithValue("@Telefono", inquilino.Telefono ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", inquilino.Email);
            command.Parameters.AddWithValue("@Direccion", inquilino.Direccion ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "UPDATE Inquilinos SET Estado = 0 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }
}
