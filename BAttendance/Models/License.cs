namespace BAttendance.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class License
    {
        [NotMapped]
        public string? Mode { get; set; }

        [Key]
        public int LicenseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CompanyUniqueId { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; } // Nullable in DB

        [Required]
        public string LicenseKeyHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserType { get; set; } = "User";

        public bool IsActive { get; set; } = true;

        public DateTime ExpirationDate { get; set; }

        [MaxLength(100)]
        public string? ActiveSessionId { get; set; } // Nullable in DB (null when no active session)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}