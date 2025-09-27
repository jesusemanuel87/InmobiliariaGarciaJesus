using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class InmuebleRepository : IRepository<Inmueble>
    {
        private readonly string _connectionString;

        public InmuebleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<Inmueble>> GetAllAsync()
        {
            var inmuebles = new List<Inmueble>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT i.Id, i.Direccion, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, 
                         i.PropietarioId, i.Tipo, i.Precio, i.Estado, i.Uso, i.FechaCreacion,
                         i.Localidad, i.Provincia
                         FROM Inmuebles i";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var inmueble = new Inmueble
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Direccion = reader["Direccion"].ToString() ?? string.Empty,
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = reader["Superficie"] == DBNull.Value ? null : Convert.ToDecimal(reader["Superficie"]),
                    Latitud = reader["Latitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Latitud"]),
                    Longitud = reader["Longitud"] == DBNull.Value ? null : Convert.ToDecimal(reader["Longitud"]),
                    PropietarioId = Convert.ToInt32(reader["PropietarioId"]),
                    Tipo = Enum.TryParse<TipoInmueble>(reader["Tipo"]?.ToString(), out var tipo) ? tipo : TipoInmueble.Casa,
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Uso = Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Localidad = reader["Localidad"]?.ToString(),
                    Provincia = reader["Provincia"]?.ToString(),
                    Disponible = true // Default to available since column doesn't exist yet
                };
                inmuebles.Add(inmueble);
            }
            
            // Load images for each inmueble using separate connections
            foreach (var inmueble in inmuebles)
            {
                await LoadImagenesAsync(inmueble);
            }
            
            return inmuebles;
        }
        
        private async Task LoadImagenesAsync(Inmueble inmueble)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT Id, InmuebleId, NombreArchivo, RutaArchivo, EsPortada, FechaCreacion 
                         FROM InmuebleImagenes 
                         WHERE InmuebleId = @InmuebleId 
                         ORDER BY EsPortada DESC, FechaCreacion ASC";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmueble.Id);
            
            using var reader = await command.ExecuteReaderAsync();
            var imagenes = new List<InmuebleImagen>();
            
            while (await reader.ReadAsync())
            {
                imagenes.Add(new InmuebleImagen
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    InmuebleId = Convert.ToInt32(reader["InmuebleId"]),
                    NombreArchivo = reader["NombreArchivo"].ToString() ?? string.Empty,
                    RutaArchivo = reader["RutaArchivo"].ToString() ?? string.Empty,
                    EsPortada = Convert.ToBoolean(reader["EsPortada"]),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                });
            }
            
            inmueble.Imagenes = imagenes;
        }

        public async Task<IEnumerable<Inmueble>> GetAllAsync(Func<Inmueble, bool> filter)
        {
            var allInmuebles = await GetAllAsync();
            return allInmuebles.Where(filter);
        }

        public async Task<Inmueble?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT i.Id, i.Direccion, i.Localidad, i.Provincia, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, 
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
                    Localidad = reader["Localidad"]?.ToString(),
                    Provincia = reader["Provincia"]?.ToString(),
                    Ambientes = Convert.ToInt32(reader["Ambientes"]),
                    Superficie = reader["Superficie"] == DBNull.Value ? null : Convert.ToDecimal(reader["Superficie"]),
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
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO Inmuebles (Direccion, Provincia, Localidad, Ambientes, Superficie, Latitud, Longitud, 
                         PropietarioId, Tipo, Precio, Estado, Uso, FechaCreacion) 
                         VALUES (@Direccion, @Provincia, @Localidad, @Ambientes, @Superficie, @Latitud, @Longitud, 
                         @PropietarioId, @Tipo, @Precio, @Estado, @Uso, @FechaCreacion);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
            command.Parameters.AddWithValue("@Provincia", inmueble.Provincia);
            command.Parameters.AddWithValue("@Localidad", inmueble.Localidad);
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
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE Inmuebles 
                         SET Direccion = @Direccion, Provincia = @Provincia, Localidad = @Localidad, Ambientes = @Ambientes, Superficie = @Superficie, 
                             Latitud = @Latitud, Longitud = @Longitud, PropietarioId = @PropietarioId, 
                             Tipo = @Tipo, Precio = @Precio, Estado = @Estado, Uso = @Uso 
                         WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", inmueble.Id);
            command.Parameters.AddWithValue("@Direccion", inmueble.Direccion);
            command.Parameters.AddWithValue("@Provincia", inmueble.Provincia);
            command.Parameters.AddWithValue("@Localidad", inmueble.Localidad);
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
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "UPDATE Inmuebles SET Estado = 0 WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<InmuebleConContrato>> GetInmueblesByPropietarioIdAsync(int propietarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
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
