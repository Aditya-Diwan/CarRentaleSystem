using CarRentalSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CarRentalSystem.Repository
{
    public class Account
    {
        private readonly DbHelper dbHelper;

        public Account(DbHelper helper)
        {
            dbHelper = helper;
        }

        // LOGIN

        public User? Login(
            string email,
            string password)
        {
            User? user = null;

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_Login",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@Email",
                    email);

                cmd.Parameters.AddWithValue(
                    "@Password",
                    password);

                con.Open();

                SqlDataReader dr =
                    cmd.ExecuteReader();

                if (dr.Read())
                {
                    user = new User
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        FullName = dr["FullName"].ToString(),
                        Email = dr["Email"].ToString(),
                        Password = dr["Password"].ToString(),
                        Role = dr["Role"].ToString(),
                        IsBlocked = Convert.ToBoolean(dr["IsBlocked"]),
                        IsVerified = Convert.ToBoolean(dr["IsVerified"])
                    };
                }
            }

            return user;
        }

        // REGISTER

        public void Register(User user)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_RegisterUser",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@FullName",
                    user.FullName);

                cmd.Parameters.AddWithValue(
                    "@Email",
                    user.Email);

                cmd.Parameters.AddWithValue(
                    "@Password",
                    user.Password);

                cmd.Parameters.AddWithValue(
                    "@Role",
                    "User");

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}