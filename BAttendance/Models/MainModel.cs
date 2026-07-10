using System.ComponentModel.DataAnnotations;

namespace BAttendance.Models
{
    public class BranchViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(100)]
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        [Required]
        [Range(10, 2000, ErrorMessage = "Radius should be between 10 and 2000 meters")]
        [Display(Name = "Allowed Check-in Radius (meters)")]
        public int AllowedRadiusMeters { get; set; } = 150;

        [Required]
        public string Status { get; set; } = "active";
    }

    public class StaffViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Staff Code is required")]
        [StringLength(20, ErrorMessage = "Staff Code cannot exceed 20 characters")]
        [Display(Name = "Staff Code")]
        public string StaffCode { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        // This property handles the file upload in the UI
        [Display(Name = "Profile Image")]
        public IFormFile FaceImage { get; set; }

        // Stores the actual binary data for the face recognition model
        public byte[] FaceEmbedding { get; set; }

        [StringLength(50)]
        public string Role { get; set; }

        [Display(Name = "Home Branch")]
        public Guid? HomeBranchId { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
