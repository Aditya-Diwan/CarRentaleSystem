using Microsoft.Data.SqlClient;
using System.Data;

namespace CarRentalSystem.Repository
{
    public class VerificationService
    {
        private readonly DbHelper dbHelper;

        public VerificationService(DbHelper helper)
        {
            dbHelper = helper;
        }

        // ADD VERIFICATION

        public void AddVerification(
            int userId,
            byte[] governmentId,
            byte[] licenseFront,
            byte[] licenseBack,
            byte[] digitalSignature)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_AddVerification",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@UserId",
                    userId);

                cmd.Parameters.AddWithValue(
                    "@GovernmentId",
                    governmentId);

                cmd.Parameters.AddWithValue(
                    "@LicenseFront",
                    licenseFront);

                cmd.Parameters.AddWithValue(
                    "@LicenseBack",
                    licenseBack);

                cmd.Parameters.AddWithValue(
                    "@DigitalSignature",
                    digitalSignature);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // GET ALL VERIFICATIONS

        public DataTable GetVerifications()
        {
            DataTable dt =
                new DataTable();

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_GetVerifications",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                SqlDataAdapter da =
                    new SqlDataAdapter(cmd);

                da.Fill(dt);
            }

            return dt;
        }

        // APPROVE VERIFICATION

        public void ApproveVerification(int id)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_ApproveVerification",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // REJECT VERIFICATION

        public void RejectVerification(int id)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_RejectVerification",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        // DELETE VERIFICATION

        public void DeleteVerification(int id)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_DeleteVerification",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}