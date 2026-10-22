namespace CarRentalSystem.Models
{
    public class UserVerification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        // DOCUMENTS

        public byte[]? GovernmentId { get; set; }

        public byte[]? LicenseFront { get; set; }

        public byte[]? LicenseBack { get; set; }

        public byte[]? DigitalSignature { get; set; }

        // STATUS

        public string? Status { get; set; }

        public DateTime UploadDate { get; set; }

        // RELATION

        public User? User { get; set; }

        public string? Remarks { get; set; }

        public DateTime? VerifiedDate { get; set; }
    }
}