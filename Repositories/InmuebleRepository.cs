using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.Common;
using MySql.Data.MySqlClient;
using System.Text;

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
            
            var query = @"SELECT i.Id, i.Direccion, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, i.PropietarioId, i.TipoId, i.Precio, i.Estado, i.Uso, i.FechaCreacion, i.Localidad, i.Provincia,
                         t.Nombre as TipoNombre
                         FROM inmuebles i
                         LEFT JOIN TiposInmueble t ON i.TipoId = t.Id
                         WHERE i.Estado = 1";
            
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
                    TipoId = Convert.ToInt32(reader["TipoId"]),
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Uso = reader["Uso"] == DBNull.Value ? UsoInmueble.Residencial : Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
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
            
            // Los tipos ya se cargan con el JOIN en el query principal
            
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
                         i.PropietarioId, i.TipoId, i.Precio, i.Estado, i.Uso, i.FechaCreacion,
                         p.Nombre, p.Apellido, p.Telefono, p.Email, p.Direccion as PropietarioDireccion,
                         t.Nombre as TipoNombre
                         FROM inmuebles i
                         INNER JOIN propietarios p ON i.PropietarioId = p.Id
                         LEFT JOIN TiposInmueble t ON i.TipoId = t.Id
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
                    TipoId = Convert.ToInt32(reader["TipoId"]),
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Uso = reader["Uso"] == DBNull.Value ? UsoInmueble.Residencial : Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
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
                         PropietarioId, TipoId, Precio, Estado, Uso, FechaCreacion) 
                         VALUES (@Direccion, @Provincia, @Localidad, @Ambientes, @Superficie, @Latitud, @Longitud, 
                         @PropietarioId, @TipoId, @Precio, @Estado, @Uso, @FechaCreacion);
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
            command.Parameters.AddWithValue("@TipoId", inmueble.TipoId);
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
                             TipoId = @TipoId, Precio = @Precio, Estado = @Estado, Uso = @Uso 
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
            command.Parameters.AddWithValue("@TipoId", inmueble.TipoId);
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
            
            var query = @"SELECT i.Id, i.Direccion, i.TipoId, i.Uso, i.Estado, i.Precio,
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
                    Tipo = Convert.ToInt32(reader["TipoId"]) switch
                    {
                        1 => TipoInmueble.Casa,
                        2 => TipoInmueble.Departamento,
                        3 => TipoInmueble.Monoambiente,
                        4 => TipoInmueble.Local,
                        5 => TipoInmueble.Oficina,
                        6 => TipoInmueble.Terreno,
                        7 => TipoInmueble.Galpon,
                        _ => TipoInmueble.Casa
                    },
                    Uso = reader["Uso"] == DBNull.Value ? UsoInmueble.Residencial : Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var uso) ? uso : UsoInmueble.Residencial,
                    Estado = (reader["Estado"]?.ToString()?.ToLower() == "activo" || reader["Estado"]?.ToString() == "1" || reader["Estado"]?.ToString()?.ToLower() == "true") ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                    Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                    TieneContrato = contratoId != DBNull.Value,
                    EstadoContrato = contratoId == DBNull.Value ? null : 1
                };
                
                inmuebles.Add(inmueble);
            }
            
            return inmuebles;
        }

        /// <summary>
        /// Obtiene inmuebles paginados con filtros aplicados a nivel de base de datos.
        /// Este método optimiza el rendimiento al traer solo los registros necesarios.
        /// </summary>
        public async Task<PagedResult<Inmueble>> GetPagedAsync(
            int page = 1,
            int pageSize = 12,
            string? provincia = null,
            string? localidad = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            EstadoInmueble? estado = null,
            int? tipoId = null,
            UsoInmueble? uso = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construir WHERE dinámico
            var whereConditions = new List<string>();
            var parameters = new List<MySqlParameter>();

            // Filtro de estado
            if (estado.HasValue)
            {
                whereConditions.Add("i.Estado = @Estado");
                parameters.Add(new MySqlParameter("@Estado", estado.Value == EstadoInmueble.Activo ? 1 : 0));
            }
            else
            {
                // Por defecto solo mostrar activos
                whereConditions.Add("i.Estado = 1");
            }

            // Filtro de provincia
            if (!string.IsNullOrEmpty(provincia))
            {
                whereConditions.Add("i.Provincia LIKE @Provincia");
                parameters.Add(new MySqlParameter("@Provincia", $"%{provincia}%"));
            }

            // Filtro de localidad
            if (!string.IsNullOrEmpty(localidad))
            {
                whereConditions.Add("i.Localidad LIKE @Localidad");
                parameters.Add(new MySqlParameter("@Localidad", $"%{localidad}%"));
            }

            // Filtro de precio mínimo
            if (precioMin.HasValue)
            {
                whereConditions.Add("i.Precio >= @PrecioMin");
                parameters.Add(new MySqlParameter("@PrecioMin", precioMin.Value));
            }

            // Filtro de precio máximo
            if (precioMax.HasValue)
            {
                whereConditions.Add("i.Precio <= @PrecioMax");
                parameters.Add(new MySqlParameter("@PrecioMax", precioMax.Value));
            }

            // Filtro de tipo
            if (tipoId.HasValue)
            {
                whereConditions.Add("i.TipoId = @TipoId");
                parameters.Add(new MySqlParameter("@TipoId", tipoId.Value));
            }

            // Filtro de uso
            if (uso.HasValue)
            {
                whereConditions.Add("i.Uso = @Uso");
                parameters.Add(new MySqlParameter("@Uso", uso.Value.ToString()));
            }

            var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

            // Query para contar total de registros
            var countQuery = $@"
                SELECT COUNT(*) 
                FROM inmuebles i 
                {whereClause}";

            int totalCount;
            using (var countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddRange(parameters.ToArray());
                totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
            }

            // Query para obtener datos paginados
            var offset = (page - 1) * pageSize;
            var dataQuery = $@"
                SELECT i.Id, i.Direccion, i.Ambientes, i.Superficie, i.Latitud, i.Longitud, 
                       i.PropietarioId, i.TipoId, i.Precio, i.Estado, i.Uso, i.FechaCreacion, 
                       i.Localidad, i.Provincia,
                       t.Nombre as TipoNombre
                FROM inmuebles i
                LEFT JOIN TiposInmueble t ON i.TipoId = t.Id
                {whereClause}
                ORDER BY i.FechaCreacion DESC
                LIMIT @PageSize OFFSET @Offset";

            var inmuebles = new List<Inmueble>();
            using (var dataCommand = new MySqlCommand(dataQuery, connection))
            {
                dataCommand.Parameters.AddRange(parameters.Select(p => new MySqlParameter(p.ParameterName, p.Value)).ToArray());
                dataCommand.Parameters.AddWithValue("@PageSize", pageSize);
                dataCommand.Parameters.AddWithValue("@Offset", offset);

                using var reader = await dataCommand.ExecuteReaderAsync();
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
                        TipoId = Convert.ToInt32(reader["TipoId"]),
                        Precio = reader["Precio"] == DBNull.Value ? null : Convert.ToDecimal(reader["Precio"]),
                        Estado = Convert.ToBoolean(reader["Estado"]) ? EstadoInmueble.Activo : EstadoInmueble.Inactivo,
                        Uso = reader["Uso"] == DBNull.Value ? UsoInmueble.Residencial : Enum.TryParse<UsoInmueble>(reader["Uso"]?.ToString(), out var usoEnum) ? usoEnum : UsoInmueble.Residencial,
                        FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                        Localidad = reader["Localidad"]?.ToString(),
                        Provincia = reader["Provincia"]?.ToString(),
                        Disponible = true
                    };
                    inmuebles.Add(inmueble);
                }
            }

            // Cargar imágenes solo para los inmuebles de la página actual
            foreach (var inmueble in inmuebles)
            {
                await LoadImagenesAsync(inmueble);
            }

            return new PagedResult<Inmueble>(inmuebles, totalCount, page, pageSize);
        }
    }
}
