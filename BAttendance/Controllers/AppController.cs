using BAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private readonly _DbContext _context;
        private readonly IWebHostEnvironment _env;

        public AppController(_DbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: api/app/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MobileLoginRequestModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest(new { success = false, message = "Username and password are required." });
            }

            try
            {
                string hashedPassword = HashPassword(model.Password);

                var resultList = await _context.LoginResults
                    .FromSqlRaw(
                        "EXEC GET_login @Username, @Password",
                        new SqlParameter("@Username", model.Username),
                        new SqlParameter("@Password", hashedPassword)
                    )
                    .AsNoTracking()
                    .ToListAsync();

                var result = resultList.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    var token = GenerateToken(result);

                    return Ok(new
                    {
                        success = true,
                        message = "Login successful.",
                        token = token
                    });
                }
                else
                {
                    string failMessage = result?.Message ?? "Invalid username or password.";
                    return BadRequest(new { success = false, message = failMessage });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred during login: {ex.Message}" });
            }
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private string GenerateToken(LoginResult result)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        [HttpPost("RegisterFace")]
        public async Task<IActionResult> SaveFace([FromForm] StaffFaceFormRequestModel model)
        {
            return await ProcessFaceTransactionWithPhoto(model, "Save");
        }

        // POST: api/app/register (Strict Add)
        [HttpPost("register")]
        public async Task<IActionResult> RegisterFace([FromForm] StaffFaceFormRequestModel model)
        {
            return await ProcessFaceTransactionWithPhoto(model, "Add");
        }

        // POST: api/app/VerifyFace
        [HttpPost("VerifyFace")]
        public async Task<IActionResult> VerifyFace([FromBody] StaffFaceVerifyModel model)
        {
            if (string.IsNullOrWhiteSpace(model.StaffId) || string.IsNullOrWhiteSpace(model.FaceEmbedding))
            {
                return BadRequest(new { success = false, message = "StaffId and FaceEmbedding are required." });
            }

            try
            {
                var dbFaceList = await _context.Set<StaffFaceEntity>()
                    .FromSqlRaw(
                        "EXEC GET_Staff_FaceByID @StaffID",
                        new SqlParameter("@StaffID", model.StaffId)
                    )
                    .AsNoTracking()
                    .ToListAsync();

                var dbFace = dbFaceList.FirstOrDefault(f => f.IsActive);

                if (dbFace == null)
                {
                    return NotFound(new { success = false, message = "Staff face record not found or inactive." });
                }

                float[] storedVector = JsonSerializer.Deserialize<float[]>(dbFace.FaceEmbedding);
                float[] incomingVector = JsonSerializer.Deserialize<float[]>(model.FaceEmbedding);

                double similarity = CalculateCosineSimilarity(storedVector, incomingVector);
                double threshold = 0.80; // Match threshold

                if (similarity >= threshold)
                {
                    return Ok(new { success = true, message = "Face verified successfully.", similarity });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Face match failed.", similarity });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Verification error: {ex.Message}" });
            }
        }

        private async Task<IActionResult> ProcessFaceTransactionWithPhoto(StaffFaceFormRequestModel model, string tranType)
        {
            if (string.IsNullOrWhiteSpace(model.StaffId) || string.IsNullOrWhiteSpace(model.FaceEmbedding))
            {
                return BadRequest(new { success = false, message = "StaffId and FaceEmbedding are required." });
            }

            try
            {
                string? savedImagePath = null;

                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "Picture");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = $"{model.StaffId}_{Guid.NewGuid():N}_{Path.GetFileName(model.ProfileImage.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(fileStream);
                    }

                    savedImagePath = $"/Picture/{uniqueFileName}";
                }

                var faceObject = new
                {
                    Mode = tranType,
                    StaffId = model.StaffId,
                    FaceEmbedding = model.FaceEmbedding,
                    ImageProfileUrl = savedImagePath ?? model.ImageProfileUrl,
                    IsActive = true
                };

                string jsonBody = JsonSerializer.Serialize(faceObject);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerStaffFace @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "StaffFace"),
                        new SqlParameter("@TranType", tranType),
                        new SqlParameter("@EntryPrimary", model.StaffId),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    return Ok(new { success = true, message = result.Message, imagePath = savedImagePath ?? model.ImageProfileUrl });
                }
                else
                {
                    return BadRequest(new { success = false, message = result?.Message ?? "Operation failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Operation failed: {ex.Message}" });
            }
        }

        private double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
        {
            if (vectorA == null || vectorB == null || vectorA.Length != vectorB.Length)
                return 0;

            double dotProduct = 0, magnitudeA = 0, magnitudeB = 0;

            for (int i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += Math.Pow(vectorA[i], 2);
                magnitudeB += Math.Pow(vectorB[i], 2);
            }

            magnitudeA = Math.Sqrt(magnitudeA);
            magnitudeB = Math.Sqrt(magnitudeB);

            if (magnitudeA == 0 || magnitudeB == 0) return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

        // POST: api/app/checkin
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceRequestModel model)
        {
            return await ExecuteAttendanceProc("Attendance", "CheckIn", null, model);
        }

        // POST: api/app/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] AttendanceRequestModel model)
        {
            return await ExecuteAttendanceProc("Attendance", "CheckOut", null, model);
        }

        private async Task<IActionResult> ExecuteAttendanceProc(string masterType, string tranType, string? entryPrimary, AttendanceRequestModel model)
        {
            try
            {
                var jsonBody = JsonSerializer.Serialize(model);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerAttendance @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", masterType),
                        new SqlParameter("@TranType", tranType),
                        new SqlParameter("@EntryPrimary", string.IsNullOrEmpty(entryPrimary) ? (object)DBNull.Value : entryPrimary),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result ?? new SpResult { Code = 400, Message = "Unknown error occurred." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SpResult { Code = 500, Message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/app/remoterequest/submit
        [HttpPost("remoterequest/submit")]
        public async Task<IActionResult> SubmitRemoteRequest([FromBody] RemoteRequestModel model)
        {
            return await ExecuteRemoteRequestProc("RemoteRequest", "Submit", null, model);
        }

        // POST: api/app/remoterequest/approve/{id}
        [HttpPost("remoterequest/approve/{id}")]
        public async Task<IActionResult> ApproveRemoteRequest(string id, [FromBody] RemoteRequestModel model)
        {
            return await ExecuteRemoteRequestProc("RemoteRequest", "Approve", id, model);
        }

        // POST: api/app/remoterequest/reject/{id}
        [HttpPost("remoterequest/reject/{id}")]
        public async Task<IActionResult> RejectRemoteRequest(string id, [FromBody] RemoteRequestModel model)
        {
            return await ExecuteRemoteRequestProc("RemoteRequest", "Reject", id, model);
        }

        private async Task<IActionResult> ExecuteRemoteRequestProc(string masterType, string tranType, string? entryPrimary, RemoteRequestModel model)
        {
            try
            {
                var jsonBody = JsonSerializer.Serialize(model);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerRemoteRequest @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", masterType),
                        new SqlParameter("@TranType", tranType),
                        new SqlParameter("@EntryPrimary", string.IsNullOrEmpty(entryPrimary) ? (object)DBNull.Value : entryPrimary),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result ?? new SpResult { Code = 400, Message = "Unknown error occurred." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SpResult { Code = 500, Message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost("BranchList")]
        public async Task<IActionResult> BranchList()
        {
            var branchList = new List<BranchList>();

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_Branch_for_Staff";

                // Ensure connection is open
                if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                {
                    await _context.Database.OpenConnectionAsync();
                }

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    branchList.Add(new BranchList
                    {
                        DocEntry = reader["DocEntry"] != DBNull.Value ? Convert.ToInt32(reader["DocEntry"]) : 0,
                        BranchName = reader["BranchName"]?.ToString() ?? "",
                        Address = reader["Address"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load branch dropdown list: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
            finally
            {
                // Always good practice to close the connection when done, 
                // especially when using raw ADO.NET commands alongside EF Core context
                await _context.Database.CloseConnectionAsync();
            }

            return Ok(branchList);
        }


        [HttpPost("StaffEnableSetting")]
        public async Task<IActionResult> GetStaffEnableSetting([FromQuery] int branch, [FromQuery] Guid currentStaff)
        {
            try
            {
                var settings = await _context.Set<StaffEnableSettingResult>()
                    .FromSqlRaw("EXEC dbo.GET_StaffEnableSetting @Branch, @CurrentStaff",
                        new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch),
                        new Microsoft.Data.SqlClient.SqlParameter("@CurrentStaff", currentStaff))
                    .AsNoTracking()
                    .ToListAsync();

                var result = settings.FirstOrDefault();

                if (result == null)
                {
                    return NotFound(new { message = "No settings found for this branch and staff." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load staff enable setting: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("ConfigurationSetting")]
        public async Task<IActionResult> ConfigurationSetting([FromQuery] int branch, [FromQuery] Guid currentStaff)
        {
            try
            {
                var settings = await _context.Set<ConfigurationSettingResult>()
                    .FromSqlRaw("EXEC dbo.GET_ConfigurationSetting @Branch, @CurrentStaff",
                        new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch),
                        new Microsoft.Data.SqlClient.SqlParameter("@CurrentStaff", currentStaff))
                    .AsNoTracking()
                    .ToListAsync();

                var result = settings.FirstOrDefault();

                if (result == null)
                {
                    return NotFound(new { message = "No configuration found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed configuration: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("CheckInStatus")]
        public async Task<IActionResult> GetCheckInStatus([FromQuery] string staffId)
        {
            try
            {
                var statusList = await _context.Set<StaffAttendanceStatusModel>()
                    .FromSqlRaw("EXEC dbo.GET_Checkin_status @StaffId",
                        new Microsoft.Data.SqlClient.SqlParameter("@StaffId", string.IsNullOrEmpty(staffId) ? DBNull.Value : staffId))
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(statusList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load check-in status: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}