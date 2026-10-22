using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? Role { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsVerified { get; set; }

        // =========================
        // Profile Information
        // =========================

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }

        public string? DrivingLicenseNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Store profile photo in database
        public byte[]? ProfileImage { get; set; }

        // Optional - display when the user registered
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}