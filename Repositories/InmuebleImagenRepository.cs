using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class InmuebleImagenRepository : IRepository<InmuebleImagen>
    {
        private readonly MySqlConnectionManager _connectionManager;

        public InmuebleImagenRepository(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<InmuebleImagen>> GetAllAsync()
        {
            var imagenes = new List<InmuebleImagen>();
            
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, 
                         Descripcion, TamanoBytes, TipoMime, FechaCreacion, FechaActualizacion 
                         FROM InmuebleImagenes ORDER BY InmuebleId, EsPortada DESC, FechaCreacion";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                imagenes.Add(MapFromReader(reader));
            }
            
            return imagenes;
        }

        public async Task<InmuebleImagen?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, 
                         Descripcion, TamanoBytes, TipoMime, FechaCreacion, FechaActualizacion 
                         FROM InmuebleImagenes WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<IEnumerable<InmuebleImagen>> GetByInmuebleIdAsync(int inmuebleId)
        {
            var imagenes = new List<InmuebleImagen>();
            
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, 
                         Descripcion, TamanoBytes, TipoMime, FechaCreacion, FechaActualizacion 
                         FROM InmuebleImagenes WHERE InmuebleId = @InmuebleId 
                         ORDER BY EsPortada DESC, FechaCreacion";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmuebleId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                imagenes.Add(MapFromReader(reader));
            }
            
            return imagenes;
        }

        public async Task<InmuebleImagen?> GetPortadaByInmuebleIdAsync(int inmuebleId)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, 
                         Descripcion, TamanoBytes, TipoMime, FechaCreacion, FechaActualizacion 
                         FROM InmuebleImagenes WHERE InmuebleId = @InmuebleId AND EsPortada = TRUE 
                         LIMIT 1";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmuebleId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }
            
            return null;
        }

        public async Task<int> CreateAsync(InmuebleImagen imagen)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"INSERT INTO InmuebleImagenes 
                         (InmuebleId, NombreArchivo, RutaArchivo, EsPortada, Descripcion, TamanoBytes, TipoMime) 
                         VALUES (@InmuebleId, @NombreArchivo, @RutaArchivo, @EsPortada, @Descripcion, @TamanoBytes, @TipoMime);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", imagen.InmuebleId);
            command.Parameters.AddWithValue("@NombreArchivo", imagen.NombreArchivo);
            command.Parameters.AddWithValue("@RutaArchivo", imagen.RutaArchivo);
            command.Parameters.AddWithValue("@EsPortada", imagen.EsPortada);
            command.Parameters.AddWithValue("@Descripcion", imagen.Descripcion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TamanoBytes", imagen.TamanoBytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TipoMime", imagen.TipoMime ?? (object)DBNull.Value);
            
            var id = Convert.ToInt32(await command.ExecuteScalarAsync());
            imagen.Id = id;
            
            return id;
        }

        public async Task<bool> UpdateAsync(InmuebleImagen imagen)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"UPDATE InmuebleImagenes SET 
                         InmuebleId = @InmuebleId, 
                         NombreArchivo = @NombreArchivo, 
                         RutaArchivo = @RutaArchivo, 
                         EsPortada = @EsPortada, 
                         Descripcion = @Descripcion, 
                         TamanoBytes = @TamanoBytes, 
                         TipoMime = @TipoMime,
                         FechaActualizacion = NOW()
                         WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", imagen.Id);
            command.Parameters.AddWithValue("@InmuebleId", imagen.InmuebleId);
            command.Parameters.AddWithValue("@NombreArchivo", imagen.NombreArchivo);
            command.Parameters.AddWithValue("@RutaArchivo", imagen.RutaArchivo);
            command.Parameters.AddWithValue("@EsPortada", imagen.EsPortada);
            command.Parameters.AddWithValue("@Descripcion", imagen.Descripcion ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TamanoBytes", imagen.TamanoBytes ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@TipoMime", imagen.TipoMime ?? (object)DBNull.Value);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "DELETE FROM InmuebleImagenes WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> SetPortadaAsync(int imagenId, int inmuebleId)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            
            try
            {
                // Quitar portada de todas las imágenes del inmueble
                var query1 = "UPDATE InmuebleImagenes SET EsPortada = FALSE WHERE InmuebleId = @InmuebleId";
                using var command1 = new MySqlCommand(query1, connection, transaction);
                command1.Parameters.AddWithValue("@InmuebleId", inmuebleId);
                await command1.ExecuteNonQueryAsync();
                
                // Establecer nueva portada
                var query2 = "UPDATE InmuebleImagenes SET EsPortada = TRUE WHERE Id = @Id";
                using var command2 = new MySqlCommand(query2, connection, transaction);
                command2.Parameters.AddWithValue("@Id", imagenId);
                var rowsAffected = await command2.ExecuteNonQueryAsync();
                
                await transaction.CommitAsync();
                return rowsAffected > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private InmuebleImagen MapFromReader(IDataReader reader)
        {
            return new InmuebleImagen
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                InmuebleId = reader.GetInt32(reader.GetOrdinal("InmuebleId")),
                NombreArchivo = reader.GetString(reader.GetOrdinal("NombreArchivo")),
                RutaArchivo = reader.GetString(reader.GetOrdinal("RutaArchivo")),
                EsPortada = reader.GetBoolean(reader.GetOrdinal("EsPortada")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                TamanoBytes = reader.IsDBNull(reader.GetOrdinal("TamanoBytes")) ? null : reader.GetInt64(reader.GetOrdinal("TamanoBytes")),
                TipoMime = reader.IsDBNull(reader.GetOrdinal("TipoMime")) ? null : reader.GetString(reader.GetOrdinal("TipoMime")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                FechaActualizacion = reader.GetDateTime(reader.GetOrdinal("FechaActualizacion"))
            };
        }
    }
}
