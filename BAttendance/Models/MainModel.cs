using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BAttendance.Models
{

    public class SpResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }

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

       

        public string IsMonday { get; set; } = "Y";
        public TimeSpan OpeningTimeMonday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeMonday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsTuesday { get; set; } = "Y";

        public TimeSpan OpeningTimeTuesday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeTuesday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsWednesday { get; set; } = "Y";

        public TimeSpan OpeningTimeWednesday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeWednesday { get; set; } = new TimeSpan(17, 0, 0);
        public string IsThursday { get; set; } = "Y";

        public TimeSpan OpeningTimeThursda { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeThursda { get; set; } = new TimeSpan(17, 0, 0);
        public string IsFriday { get; set; } = "Y";
        public TimeSpan OpeningTimeFriday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeFriday { get; set; } = new TimeSpan(17, 0, 0);
        public string IsSaturday { get; set; } = "N";
        public TimeSpan OpeningTimeSaturday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeSaturday { get; set; } = new TimeSpan(17, 0, 0);
        public string IsSunday { get; set; } = "N";
        public TimeSpan OpeningTimeSunday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeSunday { get; set; } = new TimeSpan(17, 0, 0);

        [Display(Name = "Restrict by Company WiFi Public IP")]
        public bool IsIpRestrictionEnabled { get; set; } = false;

        // Optional: If your database strictly requires "Y" / "N" strings, 
        // you can expose a helper property for database mapping:
        public string IsIpRestrictionEnabledDb => IsIpRestrictionEnabled ? "Y" : "N";

        [StringLength(45)]
        [Display(Name = "Company Public IP Address")]
        public string? CompanyPublicIP { get; set; }
    }


    public class StaffViewModel
    {
        public string? Mode { get; set; }
        public Guid Id { get; set; }

        // --- Identity ---
        [Required] public string StaffCode { get; set; }
        [Required] public string FullName { get; set; }
        public string? NationalId { get; set; }

        public DateTime? JoinDate { get; set; }

        // --- Contact ---
        [EmailAddress] public string Email { get; set; }
        public string Phone { get; set; }

        // --- Organization ---
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
        public Guid? HomeBranchId { get; set; }

        [Display(Name = "Enable Custom Working Hours for Staff")]
        public bool IsCustomScheduleEnabled { get; set; } = false;

        // --- Custom Working Hours per Day (Like Branch Setup) ---
        public string IsMonday { get; set; } = "Y";
        public TimeSpan OpeningTimeMonday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeMonday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsTuesday { get; set; } = "Y";
        public TimeSpan OpeningTimeTuesday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeTuesday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsWednesday { get; set; } = "Y";
        public TimeSpan OpeningTimeWednesday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeWednesday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsThursday { get; set; } = "Y";
        public TimeSpan OpeningTimeThursday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeThursday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsFriday { get; set; } = "Y";
        public TimeSpan OpeningTimeFriday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeFriday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsSaturday { get; set; } = "N";
        public TimeSpan OpeningTimeSaturday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeSaturday { get; set; } = new TimeSpan(17, 0, 0);

        public string IsSunday { get; set; } = "N";
        public TimeSpan OpeningTimeSunday { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan ClosingTimeSunday { get; set; } = new TimeSpan(17, 0, 0);

        // --- System & Status ---
        [Display(Name = "Linked User Account")]
        public string? UserId { get; set; } // Added field to link Staff profile to an application User ID

        public string Role { get; set; } = "Staff";
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }


    public class UserViewModel
    {


        [Required]
        [StringLength(100)]
        public string Usercode { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        // We store the hash, never the plain text password
        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; }

        public string? StaffId { get; set; }

        public string Status { get; set; } = "active";

        public string IsLicense { get; set; } = "N";


        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- New Security Policies Options (Enabled by default) ---
        public bool RequireGpsLocation { get; set; } = true;
        public bool RequireFaceScan { get; set; } = true;
        public bool RequirePublicIp { get; set; } = true;


    }


    public class LoginResult
    {
        public int Code { get; set; }                // 200
        public string? Message { get; set; }         // "Success Login"

        public int? UserID { get; set; }
        public string? UserCode { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? UserRole { get; set; }
    }
}
