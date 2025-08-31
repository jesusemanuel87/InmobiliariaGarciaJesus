using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class ConfiguracionRepository : IRepository<Configuracion>
    {
        private readonly MySqlConnectionManager _connectionManager;

        public ConfiguracionRepository(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<Configuracion>> GetAllAsync()
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT Id, Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion 
                         FROM configuraciones 
                         ORDER BY Tipo, Clave";

            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var configuraciones = new List<Configuracion>();
            while (await reader.ReadAsync())
            {
                configuraciones.Add(new Configuracion
                {
                    Id = reader.GetInt32(0),
                    Clave = reader.GetString(1),
                    Valor = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tipo = (TipoConfiguracion)reader.GetInt32(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaModificacion = reader.GetDateTime(6)
                });
            }

            return configuraciones;
        }

        public async Task<Configuracion?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT Id, Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion 
                         FROM configuraciones 
                         WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Configuracion
                {
                    Id = reader.GetInt32(0),
                    Clave = reader.GetString(1),
                    Valor = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tipo = (TipoConfiguracion)reader.GetInt32(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaModificacion = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public async Task<Configuracion?> GetByClaveAsync(string clave)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT Id, Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion 
                         FROM configuraciones 
                         WHERE Clave = @Clave";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Clave", clave);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Configuracion
                {
                    Id = reader.GetInt32(0),
                    Clave = reader.GetString(1),
                    Valor = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tipo = (TipoConfiguracion)reader.GetInt32(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaModificacion = reader.GetDateTime(6)
                };
            }

            return null;
        }

        public async Task<int> CreateAsync(Configuracion configuracion)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"INSERT INTO configuraciones (Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion) 
                         VALUES (@Clave, @Valor, @Descripcion, @Tipo, @FechaCreacion, @FechaModificacion);
                         SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Clave", configuracion.Clave);
            command.Parameters.AddWithValue("@Valor", configuracion.Valor);
            command.Parameters.AddWithValue("@Descripcion", configuracion.Descripcion);
            command.Parameters.AddWithValue("@Tipo", (int)configuracion.Tipo);
            command.Parameters.AddWithValue("@FechaCreacion", configuracion.FechaCreacion);
            command.Parameters.AddWithValue("@FechaModificacion", configuracion.FechaModificacion);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Configuracion configuracion)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"UPDATE configuraciones 
                         SET Clave = @Clave, Valor = @Valor, Descripcion = @Descripcion, 
                             Tipo = @Tipo, FechaModificacion = @FechaModificacion 
                         WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", configuracion.Id);
            command.Parameters.AddWithValue("@Clave", configuracion.Clave);
            command.Parameters.AddWithValue("@Valor", configuracion.Valor);
            command.Parameters.AddWithValue("@Descripcion", configuracion.Descripcion);
            command.Parameters.AddWithValue("@Tipo", (int)configuracion.Tipo);
            command.Parameters.AddWithValue("@FechaModificacion", DateTime.Now);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = "DELETE FROM configuraciones WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Configuracion>> GetByTipoAsync(TipoConfiguracion tipo)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();

            var query = @"SELECT Id, Clave, Valor, Descripcion, Tipo, FechaCreacion, FechaModificacion 
                         FROM configuraciones 
                         WHERE Tipo = @Tipo 
                         ORDER BY Clave";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Tipo", (int)tipo);

            using var reader = await command.ExecuteReaderAsync();

            var configuraciones = new List<Configuracion>();
            while (await reader.ReadAsync())
            {
                configuraciones.Add(new Configuracion
                {
                    Id = reader.GetInt32(0),
                    Clave = reader.GetString(1),
                    Valor = reader.GetString(2),
                    Descripcion = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tipo = (TipoConfiguracion)reader.GetInt32(4),
                    FechaCreacion = reader.GetDateTime(5),
                    FechaModificacion = reader.GetDateTime(6)
                });
            }

            return configuraciones;
        }
    }
}
