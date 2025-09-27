using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class TipoInmuebleRepository : IRepository<TipoInmuebleEntity>
    {
        private readonly string _connectionString;

        public TipoInmuebleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<TipoInmuebleEntity>> GetAllAsync()
        {
            var tipos = new List<TipoInmuebleEntity>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Id, Nombre, Descripcion, Estado, FechaCreacion FROM TiposInmueble ORDER BY Nombre";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tipos.Add(new TipoInmuebleEntity
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Descripcion = reader["Descripcion"]?.ToString(),
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                });
            }

            return tipos;
        }

        public async Task<IEnumerable<TipoInmuebleEntity>> GetAllAsync(Func<TipoInmuebleEntity, bool> filter)
        {
            var allTipos = await GetAllAsync();
            return allTipos.Where(filter);
        }

        public async Task<TipoInmuebleEntity?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Id, Nombre, Descripcion, Estado, FechaCreacion FROM TiposInmueble WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new TipoInmuebleEntity
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Nombre = reader["Nombre"].ToString() ?? string.Empty,
                    Descripcion = reader["Descripcion"]?.ToString(),
                    Estado = Convert.ToBoolean(reader["Estado"]),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                };
            }

            return null;
        }

        public async Task<int> CreateAsync(TipoInmuebleEntity entity)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO TiposInmueble (Nombre, Descripcion, Estado, FechaCreacion) 
                         VALUES (@Nombre, @Descripcion, @Estado, @FechaCreacion);
                         SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", entity.Nombre);
            command.Parameters.AddWithValue("@Descripcion", entity.Descripcion);
            command.Parameters.AddWithValue("@Estado", entity.Estado);
            command.Parameters.AddWithValue("@FechaCreacion", entity.FechaCreacion);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(TipoInmuebleEntity entity)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE TiposInmueble 
                         SET Nombre = @Nombre, Descripcion = @Descripcion, Estado = @Estado 
                         WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", entity.Id);
            command.Parameters.AddWithValue("@Nombre", entity.Nombre);
            command.Parameters.AddWithValue("@Descripcion", entity.Descripcion);
            command.Parameters.AddWithValue("@Estado", entity.Estado);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Verificar si hay inmuebles usando este tipo
            var checkQuery = "SELECT COUNT(*) FROM inmuebles WHERE TipoId = @Id";
            using var checkCommand = new MySqlCommand(checkQuery, connection);
            checkCommand.Parameters.AddWithValue("@Id", id);
            var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count > 0)
            {
                throw new InvalidOperationException($"No se puede eliminar el tipo porque hay {count} inmuebles que lo utilizan.");
            }

            var query = "DELETE FROM TiposInmueble WHERE Id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<TipoInmuebleEntity>> GetActivosAsync()
        {
            return await GetAllAsync(t => t.Estado == true);
        }

        public async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT COUNT(*) FROM TiposInmueble WHERE Nombre = @Nombre";
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nombre", nombre);
            if (excludeId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }
    }
}
