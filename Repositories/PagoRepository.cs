using InmobiliariaGarciaJesus.Models;
using MySql.Data.MySqlClient;

namespace InmobiliariaGarciaJesus.Repositories
{
    public class PagoRepository : IRepository<Pago>
    {
        private readonly string _connectionString;

        public PagoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<IEnumerable<Pago>> GetAllAsync()
        {
            var pagos = new List<Pago>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT Id, Numero, ContratoId, Importe, Intereses, Multas, 
                         FechaVencimiento, FechaPago, Estado, metodo_pago, observaciones, FechaCreacion
                         FROM Pagos";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                pagos.Add(new Pago
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Numero = Convert.ToInt32(reader["Numero"]),
                    ContratoId = Convert.ToInt32(reader["ContratoId"]),
                    Importe = Convert.ToDecimal(reader["Importe"]),
                    Intereses = Convert.ToDecimal(reader["Intereses"]),
                    Multas = Convert.ToDecimal(reader["Multas"]),
                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                    FechaPago = reader["FechaPago"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaPago"]),
                    Estado = Enum.TryParse<EstadoPago>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoPago.Pendiente,
                    MetodoPago = reader["metodo_pago"] == DBNull.Value ? null : Enum.TryParse<MetodoPago>(reader["metodo_pago"]?.ToString(), out var metodo) ? metodo : (MetodoPago?)null,
                    Observaciones = reader["observaciones"]?.ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                });
            }
            
            return pagos;
        }

        public async Task<IEnumerable<Pago>> GetAllAsync(Func<Pago, bool> filter)
        {
            var allPagos = await GetAllAsync();
            return allPagos.Where(filter);
        }

        public async Task<Pago?> GetByIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT Id, Numero, FechaPago, ContratoId, Importe, Intereses, Multas, FechaVencimiento, Estado, FechaCreacion, metodo_pago, observaciones, CreadoPorId, AnuladoPorId, FechaAnulacion
                         FROM pagos 
                         WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Pago
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Numero = Convert.ToInt32(reader["Numero"]),
                    FechaPago = reader["FechaPago"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaPago"]),
                    ContratoId = Convert.ToInt32(reader["ContratoId"]),
                    Importe = Convert.ToDecimal(reader["Importe"]),
                    Intereses = reader["Intereses"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Intereses"]),
                    Multas = reader["Multas"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Multas"]),
                    FechaVencimiento = reader["FechaVencimiento"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["FechaVencimiento"]),
                    Estado = Enum.TryParse<EstadoPago>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoPago.Pendiente,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    MetodoPago = reader["metodo_pago"] == DBNull.Value ? null : Enum.TryParse<MetodoPago>(reader["metodo_pago"]?.ToString(), out var metodo) ? metodo : (MetodoPago?)null,
                    Observaciones = reader["observaciones"] == DBNull.Value ? null : reader["observaciones"]?.ToString(),
                    CreadoPorId = reader["CreadoPorId"] != DBNull.Value ? Convert.ToInt32(reader["CreadoPorId"]) : null,
                    AnuladoPorId = reader["AnuladoPorId"] != DBNull.Value ? Convert.ToInt32(reader["AnuladoPorId"]) : null,
                    FechaAnulacion = reader["FechaAnulacion"] != DBNull.Value ? Convert.ToDateTime(reader["FechaAnulacion"]) : null
                };
            }

            return null;
        }

        public async Task<int> CreateAsync(Pago pago)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO pagos (Numero, FechaPago, ContratoId, Importe, Intereses, Multas, FechaVencimiento, Estado, FechaCreacion)
                         VALUES (@Numero, @FechaPago, @ContratoId, @Importe, @Intereses, @Multas, @FechaVencimiento, @Estado, @FechaCreacion);
                         SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Numero", pago.Numero);
            command.Parameters.AddWithValue("@FechaPago", pago.FechaPago.HasValue ? pago.FechaPago.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ContratoId", pago.ContratoId);
            command.Parameters.AddWithValue("@Importe", pago.Importe);
            command.Parameters.AddWithValue("@Intereses", pago.Intereses);
            command.Parameters.AddWithValue("@Multas", pago.Multas);
            command.Parameters.AddWithValue("@FechaVencimiento", pago.FechaVencimiento != DateTime.MinValue ? pago.FechaVencimiento : DBNull.Value);
            command.Parameters.AddWithValue("@Estado", pago.Estado.ToString());
            command.Parameters.AddWithValue("@FechaCreacion", pago.FechaCreacion);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Pago pago)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE pagos 
                         SET Numero = @Numero, FechaPago = @FechaPago, ContratoId = @ContratoId, 
                             Importe = @Importe, Intereses = @Intereses, Multas = @Multas, FechaVencimiento = @FechaVencimiento,
                             Estado = @Estado, metodo_pago = @MetodoPago, observaciones = @Observaciones
                         WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", pago.Id);
            command.Parameters.AddWithValue("@Numero", pago.Numero);
            command.Parameters.AddWithValue("@FechaPago", pago.FechaPago.HasValue ? pago.FechaPago.Value : DBNull.Value);
            command.Parameters.AddWithValue("@ContratoId", pago.ContratoId);
            command.Parameters.AddWithValue("@Importe", pago.Importe);
            command.Parameters.AddWithValue("@Intereses", pago.Intereses);
            command.Parameters.AddWithValue("@Multas", pago.Multas);
            command.Parameters.AddWithValue("@FechaVencimiento", pago.FechaVencimiento != DateTime.MinValue ? pago.FechaVencimiento : DBNull.Value);
            command.Parameters.AddWithValue("@Estado", pago.Estado.ToString());
            command.Parameters.AddWithValue("@MetodoPago", pago.MetodoPago.HasValue ? pago.MetodoPago.Value.ToString() : DBNull.Value);
            command.Parameters.AddWithValue("@Observaciones", !string.IsNullOrEmpty(pago.Observaciones) ? pago.Observaciones : DBNull.Value);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "DELETE FROM pagos WHERE Id = @Id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<List<Pago>> GetPagosByContratoIdAsync(int contratoId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT Id, Numero, FechaPago, ContratoId, Importe, Intereses, Multas, FechaVencimiento, Estado, FechaCreacion, metodo_pago, observaciones 
                         FROM pagos 
                         WHERE ContratoId = @ContratoId
                         ORDER BY Numero";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@ContratoId", contratoId);

            using var reader = await command.ExecuteReaderAsync();

            var pagos = new List<Pago>();

            while (await reader.ReadAsync())
            {
                var pago = new Pago
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Numero = Convert.ToInt32(reader["Numero"]),
                    FechaPago = reader["FechaPago"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaPago"]),
                    ContratoId = Convert.ToInt32(reader["ContratoId"]),
                    Importe = Convert.ToDecimal(reader["Importe"]),
                    Intereses = reader["Intereses"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Intereses"]),
                    Multas = reader["Multas"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Multas"]),
                    FechaVencimiento = reader["FechaVencimiento"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["FechaVencimiento"]),
                    Estado = Enum.TryParse<EstadoPago>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoPago.Pendiente,
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    MetodoPago = reader["metodo_pago"] == DBNull.Value ? null : Enum.TryParse<MetodoPago>(reader["metodo_pago"]?.ToString(), out var metodo) ? metodo : (MetodoPago?)null,
                    Observaciones = reader["observaciones"] == DBNull.Value ? null : reader["observaciones"]?.ToString()
                };

                pagos.Add(pago);
            }

            return pagos;
        }

        public async Task<IEnumerable<dynamic>> GetAllWithRelatedDataAsync()
        {
            var results = new List<dynamic>();
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var query = @"SELECT 
                            p.Id, p.Numero, p.ContratoId, p.Importe, p.Intereses, p.Multas, 
                            p.FechaVencimiento, p.FechaPago, p.Estado, p.metodo_pago, p.observaciones, p.FechaCreacion,
                            CONCAT(i.Nombre, ' ', i.Apellido) as InquilinoNombre,
                            inm.Direccion as InmuebleDireccion,
                            c.Estado as EstadoContrato
                         FROM Pagos p
                         INNER JOIN Contratos c ON p.ContratoId = c.Id
                         INNER JOIN Inquilinos i ON c.InquilinoId = i.Id
                         INNER JOIN Inmuebles inm ON c.InmuebleId = inm.Id";
            
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Numero = Convert.ToInt32(reader["Numero"]),
                    ContratoId = Convert.ToInt32(reader["ContratoId"]),
                    Importe = Convert.ToDecimal(reader["Importe"]),
                    Intereses = Convert.ToDecimal(reader["Intereses"]),
                    Multas = Convert.ToDecimal(reader["Multas"]),
                    FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                    FechaPago = reader["FechaPago"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaPago"]),
                    Estado = Enum.TryParse<EstadoPago>(reader["Estado"]?.ToString(), out var estado) ? estado : EstadoPago.Pendiente,
                    MetodoPago = reader["metodo_pago"] == DBNull.Value ? null : Enum.TryParse<MetodoPago>(reader["metodo_pago"]?.ToString(), out var metodo) ? metodo : (MetodoPago?)null,
                    Observaciones = reader["observaciones"]?.ToString(),
                    FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                    InquilinoNombre = reader["InquilinoNombre"]?.ToString(),
                    InmuebleDireccion = reader["InmuebleDireccion"]?.ToString(),
                    EstadoContrato = Enum.TryParse<EstadoContrato>(reader["EstadoContrato"]?.ToString(), out var estadoContrato) ? estadoContrato : EstadoContrato.Activo
                });
            }
            
            return results;
        }
    }
}
