namespace CarRentalSystem.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string? CarName { get; set; }

        public string? CarModel { get; set; }

        // NEW VEHICLE DETAILS

        public string? CarType { get; set; }

        public int? ManufacturingYear { get; set; }

        public string? RegistrationNumber { get; set; }

        public int? SeatingCapacity { get; set; }

        public string? Transmission { get; set; }

        public string? FuelType { get; set; }

        public string? VehicleStatus { get; set; }

        public decimal PricePerDay { get; set; }

        public byte[]? CarImage { get; set; }

        public bool Available { get; set; }

        // LOCATION DETAILS

        public string? PickupLocation { get; set; }

        public string? Address { get; set; }

        public string? Landmark { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }
    }
}