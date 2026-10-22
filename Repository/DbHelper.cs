using Microsoft.Data.SqlClient;

namespace CarRentalSystem.Repository
{
    public class DbHelper
    {
        private readonly IConfiguration configuration;

        public DbHelper(IConfiguration config)
        {
            configuration = config;
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(
                configuration.GetConnectionString(
                    "EliteCarConnection"));
        }
    }
}