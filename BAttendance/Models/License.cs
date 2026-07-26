namespace BAttendance.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class License
    {
        [Key]
        public int LicenseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [Required]
        [MaxLength(100)]
        public string CompanyUniqueId { get; set; }

        [MaxLength(150)]
        public string Location { get; set; }

        [Required]
        public string LicenseKeyHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime ExpirationDate { get; set; }

        [MaxLength(100)]
        public string ActiveSessionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
