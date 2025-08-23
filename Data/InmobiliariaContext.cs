using MySql.Data.MySqlClient;
using System.Data;

namespace InmobiliariaGarciaJesus.Data
{
    public class MySqlConnectionManager
    {
        private readonly string _connectionString;

        public MySqlConnectionManager(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
