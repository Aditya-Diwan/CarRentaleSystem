using Microsoft.Data.SqlClient;
using System.Data;

namespace CarRentalSystem.Repository
{
    public class DashboardService
    {
        private readonly DbHelper dbHelper;

        public DashboardService(DbHelper helper)
        {
            dbHelper = helper;
        }

        public DataTable GetDashboardData()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_Dashboard",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                da.Fill(dt);
            }

            return dt;
        }
    }
}