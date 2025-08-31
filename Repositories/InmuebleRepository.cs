using InmobiliariaGarciaJesus.Data;
using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class InmuebleRepository : IRepository<Inmueble>
    {
        private readonly MySqlConnectionManager _connectionManager;

        public InmuebleRepository(MySqlConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<Inmueble>> GetAllAsync()
        {
            var inmuebles = new List<Inmueble>();
            
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT Id, Direccion, Ambientes, Superficie, Latitud, Longitud, 
                         PropietarioId, Tipo, Precio, Estado, Uso, FechaCreacion 
                         FROM Inmuebles";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                inmuebles.Add(new Inmueble
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = Convert.ToDecimal(reader["Superficie"]),
                    Latitud = reader["Latitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Latitud"]),
                    Longitud = reader["Longitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Longitud"]),
                    PropietarioId = Convert.ToInt32(reader["PropietarioId"]),
                    Tipo = Enum.TryParse<TipoInmueble>(reader["Tipo"]?.ToString(), out var tipo) ? tipo : TipoInmueble.Casa,
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Uso = Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                });
            }
            
            return inmuebles;
        }

        public async Task<Inmueble?> GetByIdAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT i.Id, i.Direccion, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, 
                         i.PropietarioId, i.Tipo, i.Precio, i.Estado, i.Uso, i.FechaCreacion,
                         p.Nombre, p.Apellido, p.Telefono, p.Email, p.Direccion as PropietarioDireccion
                         FROM inmuebles i
                         INNER JOIN propietarios p ON i.PropietarioId = p.Id
                         WHERE i.Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Inmueble
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = Convert.ToDecimal(reader["Superficie"]),
                    Latitud = reader["Latitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Latitud"]),
                    Longitud = reader["Longitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Longitud"]),
                    PropietarioId = Convert.ToInt32(reader["PropietarioId"]),
                    Tipo = Enum.TryParse<TipoInmueble>(reader["Tipo"]?.ToString(), out var tipo) ? tipo : TipoInmueble.Casa,
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Uso = Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Propietario = new Propietario
                    {
                        Id = Convert.ToInt32(reader["PropietarioId"]),
                        Nombre = reader["Nombre"].ToString() ?? string.Empty,
                        Apellido = reader["Apellido"].ToString() ?? string.Empty,
                        Telefono = reader["Telefono"]?.ToString(),
                        Email = reader["Email"].ToString() ?? string.Empty,
                        Direccion = reader["PropietarioDireccion"]?.ToString()
                    }
                };
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Inmueble inmueble)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Inmuebles (Direccion, Ambientes, Superficie, Latitud, Longitud, 
                         PropietarioId, Tipo, Precio, Estado, Uso, FechaCreacion) 
                         VALUES (@Direccion, @Ambientes, @Superficie, @Latitud, @Longitud, 
                         @PropietarioId, @Tipo, @Precio, @Estado, @Uso, @FechaCreacion);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
            command.Parameters.AddWithValue("@Ambientes", inmueble.Ambientes);
            command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
            command.Parameters.AddWithValue("@Latitud", inmueble.Latitud ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Longitud", inmueble.Longitud ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PropietarioId", inmueble.PropietarioId);
            command.Parameters.AddWithValue("@Tipo", inmueble.Tipo.ToString());
            command.Parameters.AddWithValue("@Precio", inmueble.Precio ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Estado", inmueble.Estado == EstadoInmueble.Activo ? 1 : 0);
            command.Parameters.AddWithValue("@Uso", inmueble.Uso.ToString());
            command.Parameters.AddWithValue("@FechaCreacion", inmueble.FechaCreacion);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Inmueble inmueble)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"UPDATE Inmuebles 
                         SET Direccion = @Direccion, Ambientes = @Ambientes, Superficie = @Superficie, 
                             Latitud = @Latitud, Longitud = @Longitud, PropietarioId = @PropietarioId, 
                             Tipo = @Tipo, Precio = @Precio, Estado = @Estado, Uso = @Uso 
                         WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", inmueble.Id);
            command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
            command.Parameters.AddWithValue("@Ambientes", inmueble.Ambientes);
            command.Parameters.AddWithValue("@Superficie", inmueble.Superficie);
            command.Parameters.AddWithValue("@Latitud", inmueble.Latitud ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Longitud", inmueble.Longitud ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PropietarioId", inmueble.PropietarioId);
            command.Parameters.AddWithValue("@Tipo", inmueble.Tipo.ToString());
            command.Parameters.AddWithValue("@Precio", inmueble.Precio ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Estado", inmueble.Estado == EstadoInmueble.Activo ? 1 : 0);
            command.Parameters.AddWithValue("@Uso", inmueble.Uso.ToString());
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = "UPDATE Inmuebles SET Estado = 0 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<InmuebleConContrato>> GetInmueblesByPropietarioIdAsync(int propietarioId)
        {
            using var connection = _connectionManager.GetConnection();
            await connection.OpenAsync();
            
            var query = @"SELECT i.Id, i.Direccion, i.Tipo, i.Uso, i.Estado, i.Precio,
                         c.Id as ContratoId, c.Estado as EstadoContrato
                         FROM inmuebles i
                         LEFT JOIN contratos c ON i.Id = c.InmuebleId AND c.Estado = 'Activo'
                         WHERE i.PropietarioId = @PropietarioId
                         ORDER BY i.Direccion";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PropietarioId", propietarioId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            var inmuebles = new List<InmuebleConContrato>();
            
            while (await reader.ReadAsync())
            {
                // Debug values
                var contratoId = reader["ContratoId"];
                var estadoContratoValue = reader["EstadoContrato"]?.ToString();
                
                var inmueble = new InmuebleConContrato
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Tipo = Enum.TryParse<TipoInmueble>(reader["Tipo"]?.ToString(), out var tipo) ? tipo : TipoInmueble.Casa,
                    Uso = Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
                    Estado = (reader["Estado"]?.ToString()?.ToLower() == "activo" || reader["Estado"]?.ToString() == "1" || reader["Estado"]?.ToString()?.ToLower() == "true") ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    TieneContrato = contratoId != DBNull.Value,
                    EstadoContrato = contratoId == DBNull.Value ? null : 1
                };
                
                inmuebles.Add(inmueble);
            }
            
            return inmuebles;
        }
    }
}
