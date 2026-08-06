using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BAttendance.Models
{
    public class MobileLoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class AttendanceRequestModel
    {
        [Required]
        public string StaffId { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }

        public DateTime? AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [StringLength(30)]
        public string? Status { get; set; }

        public double? CheckInLatitude { get; set; }

        public double? CheckInLongitude { get; set; }

        public decimal? DistanceMeters { get; set; }

        public bool? IsLocationValid { get; set; }

        public bool? IsFaceVerified { get; set; }

        public double? FaceSimilarityScore { get; set; }

        [StringLength(45)]
        public string? CheckInIPAddress { get; set; }

        [StringLength(255)]
        public string? DeviceModel { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }


    public class RemoteRequestModel
    {
        // Optional for Admin Approve/Reject, but required for Submit
        public string? StaffId { get; set; }

        public Guid? BranchId { get; set; }

        // Optional for Admin Approve/Reject, but required for Submit
        [StringLength(20)]
        public string? RequestType { get; set; } // 'CheckIn' or 'CheckOut'

        public DateTime? RequestDate { get; set; }

        // Optional for Admin Approve/Reject, but required for Submit
        [StringLength(500)]
        public string? Reason { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        // Used for Admin actions (Approve / Reject)
        [StringLength(100)]
        public string? AdminUserId { get; set; }

        [StringLength(500)]
        public string? AdminRemarks { get; set; }
    }

    [Keyless]
    public class StaffEnableSettingResult
    {
        public string? IsWorkingDay { get; set; }
        public TimeSpan? TimeOpen { get; set; }
        public TimeSpan? TimeClose { get; set; }
        public bool? RequireGpsLocation { get; set; }
        public bool? RequireFaceScan { get; set; }
        public bool? RequirePublicIp { get; set; }
    }

}
