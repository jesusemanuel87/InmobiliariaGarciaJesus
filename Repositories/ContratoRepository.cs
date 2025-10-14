using InmobiliariaGarciaJesus.Models;
using InmobiliariaGarciaJesus.Models.Common;
using MySql.Data.MySqlClient;
using System.Text;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class ContratoRepository : IRepository<Contrato>
    {
        private readonly string _connectionString;

        public ContratoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<Contrato>> GetAllAsync()
        {
            var contratos = new List<Contrato>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT c.Id, c.FechaInicio, c.FechaFin, c.Precio, c.InquilinoId, c.InmuebleId, c.Estado, c.FechaCreacion,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido, i.DNI as InquilinoDNI,
                         i.Telefono as InquilinoTelefono, i.Email as InquilinoEmail,
                         inm.Direccion as InmuebleDireccion, inm.TipoId as InmuebleTipo, inm.Ambientes as InmuebleAmbientes
                         FROM Contratos c
                         LEFT JOIN Inquilinos i ON c.InquilinoId = i.Id
                         LEFT JOIN Inmuebles inm ON c.InmuebleId = inm.Id";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                contratos.Add(new Contrato
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    InquilinoId = Convert.ToInt32(reader["InquilinoId"]),
                    InmuebleId = Convert.ToInt32(reader["InmuebleId"]),
                    Estado = Enum.TryParse<EstadoContrato>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoContrato.Activo,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    Inquilino = new Inquilino
                    {
                        Id = Convert.ToInt32(reader["InquilinoId"]),
                        Nombre = reader["InquilinoNombre"].ToString() ?? string.Empty,
                        Apellido = reader["InquilinoApellido"].ToString() ?? string.Empty,
                        Dni = reader["InquilinoDNI"].ToString() ?? string.Empty,
                        Telefono = reader["InquilinoTelefono"]?.ToString(),
                        Email = reader["InquilinoEmail"].ToString() ?? string.Empty
                    },
                    Inmueble = new Inmueble
                    {
                        Id = Convert.ToInt32(reader["InmuebleId"]),
                        Direccion = reader["InmuebleDireccion"].ToString() ?? string.Empty,
                        TipoId = reader["InmuebleTipo"] == DBNull.Value ? 1 : Convert.ToInt32(reader["InmuebleTipo"]),
                        Ambientes = Convert.ToInt32(reader["InmuebleAmbientes"])
                    }
                });
            }
            
            return contratos;
        }

        public async Task<IEnumerable<Contrato>> GetAllAsync(Func<Contrato, bool> filter)
        {
            var allContratos = await GetAllAsync();
            return allContratos.Where(filter);
        }

        public async Task<Contrato?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT c.Id, c.FechaInicio, c.FechaFin, c.Precio, c.InquilinoId, c.InmuebleId, 
                         c.Estado, c.FechaCreacion, c.MotivoCancelacion, c.CreadoPorId, c.TerminadoPorId, c.FechaTerminacion,
                         i.Nombre as InquilinoNombre, i.Apellido as InquilinoApellido, i.DNI as InquilinoDNI,
                         i.Telefono as InquilinoTelefono, i.Email as InquilinoEmail,
                         inm.Direccion as InmuebleDireccion, inm.TipoId as InmuebleTipo, inm.Ambientes as InmuebleAmbientes
                         FROM contratos c
                         INNER JOIN inquilinos i ON c.InquilinoId = i.Id
                         INNER JOIN inmuebles inm ON c.InmuebleId = inm.Id
                         WHERE c.Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            
            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Contrato
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    InquilinoId = Convert.ToInt32(reader["InquilinoId"]),
                    InmuebleId = Convert.ToInt32(reader["InmuebleId"]),
                    Estado = Enum.TryParse<EstadoContrato>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoContrato.Activo,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    MotivoCancelacion = reader["MotivoCancelacion"]?.ToString(),
                    CreadoPorId = reader["CreadoPorId"] != DBNull.Value ? Convert.ToInt32(reader["CreadoPorId"]) : null,
                    TerminadoPorId = reader["TerminadoPorId"] != DBNull.Value ? Convert.ToInt32(reader["TerminadoPorId"]) : null,
                    FechaTerminacion = reader["FechaTerminacion"] != DBNull.Value ? Convert.ToDateTime(reader["FechaTerminacion"]) : null,
                    Inquilino = new Inquilino
                    {
                        Id = Convert.ToInt32(reader["InquilinoId"]),
                        Nombre = reader["InquilinoNombre"].ToString() ?? string.Empty,
                        Apellido = reader["InquilinoApellido"].ToString() ?? string.Empty,
                        Dni = reader["InquilinoDNI"].ToString() ?? string.Empty,
                        Telefono = reader["InquilinoTelefono"]?.ToString(),
                        Email = reader["InquilinoEmail"].ToString() ?? string.Empty
                    },
                    Inmueble = new Inmueble
                    {
                        Id = Convert.ToInt32(reader["InmuebleId"]),
                        Direccion = reader["InmuebleDireccion"].ToString() ?? string.Empty,
                        TipoId = reader["InmuebleTipo"] == DBNull.Value ? 1 : Convert.ToInt32(reader["InmuebleTipo"]),
                        Ambientes = Convert.ToInt32(reader["InmuebleAmbientes"])
                    }
                };
            }
            
            return null;
        }

        public async Task<int> CreateAsync(Contrato contrato)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"INSERT INTO contratos (FechaInicio, FechaFin, Precio, InquilinoId, InmuebleId, 
                         Estado, FechaCreacion, MotivoCancelacion) 
                         VALUES (@FechaInicio, @FechaFin, @Precio, @InquilinoId, @InmuebleId, 
                         @Estado, @FechaCreacion, @MotivoCancelacion);
                         SELECT LAST_INSERT_ID();";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@FechaInicio", contrato.FechaInicio);
            command.Parameters.AddWithValue("@FechaFin", contrato.FechaFin);
            command.Parameters.AddWithValue("@Precio", contrato.Precio);
            command.Parameters.AddWithValue("@InquilinoId", contrato.InquilinoId);
            command.Parameters.AddWithValue("@InmuebleId", contrato.InmuebleId);
            command.Parameters.AddWithValue("@Estado", contrato.Estado.ToString());
            command.Parameters.AddWithValue("@FechaCreacion", contrato.FechaCreacion);
            command.Parameters.AddWithValue("@MotivoCancelacion", contrato.MotivoCancelacion);
            
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Contrato contrato)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"UPDATE contratos 
                         SET FechaInicio = @FechaInicio, FechaFin = @FechaFin, Precio = @Precio, 
                             InquilinoId = @InquilinoId, InmuebleId = @InmuebleId, Estado = @Estado,
                             MotivoCancelacion = @MotivoCancelacion
                         WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", contrato.Id);
            command.Parameters.AddWithValue("@FechaInicio", contrato.FechaInicio);
            command.Parameters.AddWithValue("@FechaFin", contrato.FechaFin);
            command.Parameters.AddWithValue("@Precio", contrato.Precio);
            command.Parameters.AddWithValue("@InquilinoId", contrato.InquilinoId);
            command.Parameters.AddWithValue("@InmuebleId", contrato.InmuebleId);
            command.Parameters.AddWithValue("@Estado", contrato.Estado.ToString());
            command.Parameters.AddWithValue("@MotivoCancelacion", contrato.MotivoCancelacion);
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = "UPDATE contratos SET Estado = @Estado WHERE Id = @Id";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Estado", EstadoContrato.Cancelado.ToString());
            
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> HasOverlappingContractAsync(int inmuebleId, DateTime fechaInicio, DateTime fechaFin, int? excludeContratoId = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT COUNT(*) FROM contratos 
                         WHERE InmuebleId = @InmuebleId 
                         AND (Estado = 'Activo' OR Estado = 'Reservado')
                         AND NOT (
                             @FechaFin < FechaInicio OR @FechaInicio > FechaFin
                         )";
            
            if (excludeContratoId.HasValue)
            {
                query += " AND Id != @ExcludeId";
            }
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmuebleId);
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio);
            command.Parameters.AddWithValue("@FechaFin", fechaFin);
            
            if (excludeContratoId.HasValue)
            {
                command.Parameters.AddWithValue("@ExcludeId", excludeContratoId.Value);
            }
            
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<DateTime?> GetNextAvailableDateAsync(int inmuebleId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT MAX(FechaFin) FROM contratos 
                         WHERE InmuebleId = @InmuebleId 
                         AND (Estado = 'Activo' OR Estado = 'Reservado')
                         AND FechaFin >= CURDATE()";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmuebleId);
            
            var result = await command.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToDateTime(result).AddDays(1);
            }
            
            return DateTime.Today;
        }

        public async Task<List<(DateTime FechaInicio, DateTime FechaFin)>> GetUnavailableDatesAsync(int inmuebleId)
        {
            var unavailableDates = new List<(DateTime, DateTime)>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT FechaInicio, FechaFin FROM contratos 
                         WHERE InmuebleId = @InmuebleId 
                         AND (Estado = 'Activo' OR Estado = 'Reservado')
                         ORDER BY FechaInicio";
            
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InmuebleId", inmuebleId);
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                unavailableDates.Add((
                    Convert.ToDateTime(reader["FechaInicio"]),
                    Convert.ToDateTime(reader["FechaFin"])
                ));
            }
            
            return unavailableDates;
        }

        /// <summary>
        /// Obtiene contratos solo de los inmuebles especificados. 
        /// Optimiza el rendimiento al evitar cargar todos los contratos.
        /// </summary>
        public async Task<IEnumerable<Contrato>> GetByInmuebleIdsAsync(IEnumerable<int> inmuebleIds)
        {
            if (!inmuebleIds.Any())
            {
                return new List<Contrato>();
            }

            var contratos = new List<Contrato>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var idsString = string.Join(",", inmuebleIds);
            var query = $@"SELECT c.Id, c.FechaInicio, c.FechaFin, c.Precio, c.InquilinoId, c.InmuebleId, c.Estado, c.FechaCreacion
                          FROM Contratos c
                          WHERE c.InmuebleId IN ({idsString})
                          AND (c.Estado = 'Activo' OR c.Estado = 'Reservado')";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                contratos.Add(new Contrato
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                    FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                    Precio = Convert.ToDecimal(reader["Precio"]),
                    InquilinoId = Convert.ToInt32(reader["InquilinoId"]),
                    InmuebleId = Convert.ToInt32(reader["InmuebleId"]),
                    Estado = Enum.TryParse<EstadoContrato>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoContrato.Activo,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                });
            }
            
            return contratos;
        }

        /// <summary>
        /// Obtiene contratos paginados con filtros aplicados a nivel de base de datos.
        /// Soporta filtros por rol (inquilino, propietario) y múltiples criterios.
        /// </summary>
        public async Task<PagedResult<Contrato>> GetPagedAsync(
            int page = 1,
            int pageSize = 20,
            int? inquilinoId = null,
            int? propietarioId = null,
            List<EstadoContrato>? estados = null,
            string? inquilinoSearch = null,
            string? inmuebleSearch = null,
            decimal? precioMin = null,
            decimal? precioMax = null,
            DateTime? fechaInicioDesde = null,
            DateTime? fechaInicioHasta = null,
            DateTime? fechaFinDesde = null,
            DateTime? fechaFinHasta = null,
            DateTime? fechaCreacionDesde = null,
            DateTime? fechaCreacionHasta = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Construir WHERE dinámico
            var whereConditions = new List<string>();
            var parameters = new List<MySqlParameter>();

            // Filtro por inquilino (para rol Inquilino)
            if (inquilinoId.HasValue)
            {
                whereConditions.Add("c.InquilinoId = @InquilinoId");
                parameters.Add(new MySqlParameter("@InquilinoId", inquilinoId.Value));
            }

            // Filtro por propietario (para rol Propietario)
            if (propietarioId.HasValue)
            {
                whereConditions.Add("inm.PropietarioId = @PropietarioId");
                parameters.Add(new MySqlParameter("@PropietarioId", propietarioId.Value));
            }

            // Filtro por estados (multiselect)
            if (estados != null && estados.Any())
            {
                var estadosString = string.Join(",", estados.Select((_, i) => $"@Estado{i}"));
                whereConditions.Add($"c.Estado IN ({estadosString})");
                for (int i = 0; i < estados.Count; i++)
                {
                    parameters.Add(new MySqlParameter($"@Estado{i}", estados[i].ToString()));
                }
            }

            // Búsqueda por inquilino (nombre o DNI)
            if (!string.IsNullOrEmpty(inquilinoSearch))
            {
                whereConditions.Add("(CONCAT(inq.Nombre, ' ', inq.Apellido) LIKE @InquilinoSearch OR inq.DNI LIKE @InquilinoSearch)");
                parameters.Add(new MySqlParameter("@InquilinoSearch", $"%{inquilinoSearch}%"));
            }

            // Búsqueda por inmueble (dirección)
            if (!string.IsNullOrEmpty(inmuebleSearch))
            {
                whereConditions.Add("inm.Direccion LIKE @InmuebleSearch");
                parameters.Add(new MySqlParameter("@InmuebleSearch", $"%{inmuebleSearch}%"));
            }

            // Filtros de precio
            if (precioMin.HasValue)
            {
                whereConditions.Add("c.Precio >= @PrecioMin");
                parameters.Add(new MySqlParameter("@PrecioMin", precioMin.Value));
            }
            if (precioMax.HasValue)
            {
                whereConditions.Add("c.Precio <= @PrecioMax");
                parameters.Add(new MySqlParameter("@PrecioMax", precioMax.Value));
            }

            // Filtros de fecha de inicio
            if (fechaInicioDesde.HasValue)
            {
                whereConditions.Add("c.FechaInicio >= @FechaInicioDesde");
                parameters.Add(new MySqlParameter("@FechaInicioDesde", fechaInicioDesde.Value));
            }
            if (fechaInicioHasta.HasValue)
            {
                whereConditions.Add("c.FechaInicio <= @FechaInicioHasta");
                parameters.Add(new MySqlParameter("@FechaInicioHasta", fechaInicioHasta.Value));
            }

            // Filtros de fecha de fin
            if (fechaFinDesde.HasValue)
            {
                whereConditions.Add("c.FechaFin >= @FechaFinDesde");
                parameters.Add(new MySqlParameter("@FechaFinDesde", fechaFinDesde.Value));
            }
            if (fechaFinHasta.HasValue)
            {
                whereConditions.Add("c.FechaFin <= @FechaFinHasta");
                parameters.Add(new MySqlParameter("@FechaFinHasta", fechaFinHasta.Value));
            }

            // Filtros de fecha de creación
            if (fechaCreacionDesde.HasValue)
            {
                whereConditions.Add("c.FechaCreacion >= @FechaCreacionDesde");
                parameters.Add(new MySqlParameter("@FechaCreacionDesde", fechaCreacionDesde.Value));
            }
            if (fechaCreacionHasta.HasValue)
            {
                whereConditions.Add("c.FechaCreacion <= @FechaCreacionHasta");
                parameters.Add(new MySqlParameter("@FechaCreacionHasta", fechaCreacionHasta.Value));
            }

            var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";

            // Query para contar total de registros
            var countQuery = $@"
                SELECT COUNT(*) 
                FROM Contratos c
                LEFT JOIN Inquilinos inq ON c.InquilinoId = inq.Id
                LEFT JOIN Inmuebles inm ON c.InmuebleId = inm.Id
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
                SELECT c.Id, c.FechaInicio, c.FechaFin, c.Precio, c.InquilinoId, c.InmuebleId, c.Estado, c.FechaCreacion,
                       inq.Nombre as InquilinoNombre, inq.Apellido as InquilinoApellido, inq.DNI as InquilinoDNI,
                       inq.Telefono as InquilinoTelefono, inq.Email as InquilinoEmail,
                       inm.Direccion as InmuebleDireccion, inm.TipoId as InmuebleTipo, inm.Ambientes as InmuebleAmbientes
                FROM Contratos c
                LEFT JOIN Inquilinos inq ON c.InquilinoId = inq.Id
                LEFT JOIN Inmuebles inm ON c.InmuebleId = inm.Id
                {whereClause}
                ORDER BY c.FechaCreacion DESC
                LIMIT @PageSize OFFSET @Offset";

            var contratos = new List<Contrato>();
            using (var dataCommand = new MySqlCommand(dataQuery, connection))
            {
                dataCommand.Parameters.AddRange(parameters.Select(p => new MySqlParameter(p.ParameterName, p.Value)).ToArray());
                dataCommand.Parameters.AddWithValue("@PageSize", pageSize);
                dataCommand.Parameters.AddWithValue("@Offset", offset);

                using var reader = await dataCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    contratos.Add(new Contrato
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                        FechaFin = Convert.ToDateTime(reader["FechaFin"]),
                        Precio = Convert.ToDecimal(reader["Precio"]),
                        InquilinoId = Convert.ToInt32(reader["InquilinoId"]),
                        InmuebleId = Convert.ToInt32(reader["InmuebleId"]),
                        Estado = Enum.TryParse<EstadoContrato>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoContrato.Activo,
                        FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                        Inquilino = new Inquilino
                        {
                            Id = Convert.ToInt32(reader["InquilinoId"]),
                            Nombre = reader["InquilinoNombre"].ToString() ?? string.Empty,
                            Apellido = reader["InquilinoApellido"].ToString() ?? string.Empty,
                            Dni = reader["InquilinoDNI"].ToString() ?? string.Empty,
                            Telefono = reader["InquilinoTelefono"]?.ToString(),
                            Email = reader["InquilinoEmail"]?.ToString()
                        },
                        Inmueble = new Inmueble
                        {
                            Id = Convert.ToInt32(reader["InmuebleId"]),
                            Direccion = reader["InmuebleDireccion"].ToString() ?? string.Empty,
                            TipoId = reader["InmuebleTipo"] == DBNull.Value ? 0 : Convert.ToInt32(reader["InmuebleTipo"]),
                            Ambientes = reader["InmuebleAmbientes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["InmuebleAmbientes"])
                        }
                    });
                }
            }

            return new PagedResult<Contrato>(contratos, totalCount, page, pageSize);
        }
    }
}
