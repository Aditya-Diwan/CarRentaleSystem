using Microsoft.Data.SqlClient;
using System.Data;

namespace CarRentalSystem.Repository
{
    public class BookingService
    {
        private readonly DbHelper _dbHelper;

        public BookingService(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // ADD BOOKING

        public void AddBooking(
            int userId,
            int carId,
            DateTime startDate,
            DateTime endDate,
            int totalDays,
            decimal totalAmount)
        {
            using (SqlConnection con = _dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_AddBooking",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                cmd.Parameters.AddWithValue("@TotalDays", totalDays);
                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // GET ALL BOOKINGS

        public DataTable GetBookings()
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = _dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_GetBookings",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                da.Fill(dt);
            }

            return dt;
        }

        // MY BOOKINGS

        public DataTable MyBookings(int userId)
        {
            DataTable dt = new DataTable();

            using (SqlConnection con = _dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_MyBookings",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@UserId",
                    userId);

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                da.Fill(dt);
            }

            return dt;
        }
    }
}