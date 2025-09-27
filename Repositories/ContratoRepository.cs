using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

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
                         c.Estado, c.FechaCreacion, c.MotivoCancelacion,
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
    }
}
