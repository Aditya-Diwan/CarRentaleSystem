namespace CarRentalSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // FOREIGN KEYS

        public int UserId { get; set; }

        public int CarId { get; set; }

        // BOOKING DETAILS

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int TotalDays { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime BookingDate { get; set; }

        // PICKUP LOCATION SNAPSHOT

        public string? PickupLocation { get; set; }

        public string? Address { get; set; }

        public string? Landmark { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        // RETURN DETAILS

        public string? ReturnLocation { get; set; }

        public DateTime? ActualReturnDate { get; set; }

        // BOOKING STATUS

        public string Status { get; set; } = "Pending";

        public string PaymentStatus { get; set; } = "Pending";

        public string? PaymentMethod { get; set; }

        public string? CancellationReason { get; set; }

        // RELATIONS

        public User? User { get; set; }

        public Car? Car { get; set; }
    }
}