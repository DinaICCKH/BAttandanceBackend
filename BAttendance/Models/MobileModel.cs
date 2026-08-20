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
        [StringLength(100)]
        public string StaffId { get; set; } = string.Empty;

        public string? BranchId { get; set; }

        public DateTime? AttendanceDate { get; set; }

        // --- Check-In Properties ---
        public DateTime? CheckInTime { get; set; }

        [StringLength(30)]
        public string? CheckInStatus { get; set; }

        public bool? CheckInIsLate { get; set; }

        [StringLength(500)]
        public string? CheckInLateRemark { get; set; }

        public double? CheckInLatitude { get; set; }

        public double? CheckInLongitude { get; set; }

        public decimal? CheckInDistanceMeters { get; set; }

        public bool? CheckInIsLocationValid { get; set; }

        [StringLength(500)]
        public string? CheckInLocationInvalidReason { get; set; }

        public bool? CheckInIsFaceVerified { get; set; }

        public double? CheckInFaceSimilarityScore { get; set; }

        [StringLength(45)]
        public string? CheckInIPAddress { get; set; }

        public bool? CheckInIsIPValid { get; set; }

        [StringLength(500)]
        public string? CheckInIPInvalidReason { get; set; }

        [StringLength(255)]
        public string? CheckInDeviceModel { get; set; }

        // --- Check-Out Properties ---
        public DateTime? CheckOutTime { get; set; }

        [StringLength(30)]
        public string? CheckOutStatus { get; set; }

        public bool? CheckOutIsEarly { get; set; }

        [StringLength(10)]
        public string? CheckOutLateRemark { get; set; }

        public double? CheckOutLatitude { get; set; }

        public double? CheckOutLongitude { get; set; }

        public decimal? CheckOutDistanceMeters { get; set; }

        public bool? CheckOutIsLocationValid { get; set; }

        [StringLength(500)]
        public string? CheckOutLocationInvalidReason { get; set; }

        public bool? CheckOutIsFaceVerified { get; set; }

        public double? CheckOutFaceSimilarityScore { get; set; }

        [StringLength(45)]
        public string? CheckOutIPAddress { get; set; }

        public bool? CheckOutIsIPValid { get; set; }

        [StringLength(500)]
        public string? CheckOutIPInvalidReason { get; set; }

        [StringLength(255)]
        public string? CheckOutDeviceModel { get; set; }
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

    [Keyless]
    public class ConfigurationSettingResult
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? AllowedRadiusMeters { get; set; }
        public string? CompanyPublicIP { get; set; }
        public string? FaceEmbedding { get; set; } // Or byte[] if your FaceEmbedding column is stored as binary/varbinary in the database
    }


    public class StaffAttendanceStatusModel
    {
        public Guid Id { get; set; }
        public string StaffId { get; set; } = string.Empty;
        public string? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Address { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public string CheckInStatus { get; set; } = string.Empty;
        public bool? CheckInIsLate { get; set; }
        public string? CheckInLateRemark { get; set; }
        public double? CheckInLatitude { get; set; }
        public double? CheckInLongitude { get; set; }
        public decimal? CheckInDistanceMeters { get; set; }
        public bool CheckInIsLocationValid { get; set; }
        public string? CheckInLocationInvalidReason { get; set; }
        public bool CheckInIsFaceVerified { get; set; }
        public double? CheckInFaceSimilarityScore { get; set; }
        public string? CheckInIPAddress { get; set; }
        public bool? CheckInIsIPValid { get; set; }
        public string? CheckInIPInvalidReason { get; set; }
        public string? CheckInDeviceModel { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
