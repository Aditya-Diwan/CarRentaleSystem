using CarRentalSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CarRentalSystem.Repository
{
    public class CarService
    {
        private readonly DbHelper dbHelper;

        public CarService(DbHelper helper)
        {
            dbHelper = helper;
        }

        // GET ALL CARS

        public List<CarRentalSystem.Models.Car> GetCars()
        {
            List<CarRentalSystem.Models.Car> cars =
                new List<CarRentalSystem.Models.Car>();

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_GetCars",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    cars.Add(new CarRentalSystem.Models.Car
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CarName = dr["CarName"].ToString(),
                        CarModel = dr["CarModel"].ToString(),
                        Transmission = dr["Transmission"].ToString(),
                        FuelType = dr["FuelType"].ToString(),
                        PricePerDay = Convert.ToDecimal(dr["PricePerDay"]),
                        Available = Convert.ToBoolean(dr["Available"]),
                        CarImage = dr["CarImage"] == DBNull.Value
                            ? null
                            : (byte[])dr["CarImage"]
                    });
                }
            }

            return cars;
        }

        // GET AVAILABLE CARS

        public List<CarRentalSystem.Models.Car> GetAvailableCars()
        {
            List<CarRentalSystem.Models.Car> cars =
                new List<CarRentalSystem.Models.Car>();

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_GetAvailableCars",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                con.Open();

                SqlDataReader dr =
                    cmd.ExecuteReader();

                while (dr.Read())
                {
                    cars.Add(new CarRentalSystem.Models.Car
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CarName = dr["CarName"].ToString(),
                        CarModel = dr["CarModel"].ToString(),
                        Transmission = dr["Transmission"].ToString(),
                        FuelType = dr["FuelType"].ToString(),
                        PricePerDay = Convert.ToDecimal(dr["PricePerDay"]),
                        Available = Convert.ToBoolean(dr["Available"]),
                        CarImage = dr["CarImage"] == DBNull.Value
                            ? null
                            : (byte[])dr["CarImage"]
                    });
                }
            }

            return cars;
        }

        // GET CAR BY ID

        public CarRentalSystem.Models.Car? GetCarById(int id)
        {
            CarRentalSystem.Models.Car? car = null;

            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_GetCarById",
                        con);

                cmd.CommandType =
                    CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue(
                    "@Id",
                    id);

                con.Open();

                SqlDataReader dr =
                    cmd.ExecuteReader();

                if (dr.Read())
                {
                    car = new CarRentalSystem.Models.Car
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CarName = dr["CarName"].ToString(),
                        CarModel = dr["CarModel"].ToString(),
                        Transmission = dr["Transmission"].ToString(),
                        FuelType = dr["FuelType"].ToString(),
                        PricePerDay = Convert.ToDecimal(dr["PricePerDay"]),
                        Available = Convert.ToBoolean(dr["Available"]),
                        CarImage = dr["CarImage"] == DBNull.Value
                            ? null
                            : (byte[])dr["CarImage"]
                    };
                }
            }

            return car;
        }

        // DELETE CAR

        public void DeleteCar(int id)
        {
            using (SqlConnection con =
                dbHelper.GetConnection())
            {
                SqlCommand cmd =
                    new SqlCommand(
                        "sp_DeleteCar",
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