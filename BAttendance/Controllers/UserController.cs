using BAttendance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BAttendance.Controllers
{
    public class UserController : SharedController
    {
        public UserController(_DbContext context, IWebHostEnvironment env)
           : base(context, env)
        {
        }


        [HttpGet]
        public async Task<IActionResult> UserList(string? usercode = "ALL", int page = 1, int pageSize = 10)
        {
            try
            {
                usercode = string.IsNullOrEmpty(usercode) ? "ALL" : usercode;

                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_User_List @UserID, @Usercode, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@UserID", 1)); // Replace with actual logged-in user ID if available
                command.Parameters.Add(new SqlParameter("@Usercode", usercode));
                command.Parameters.Add(new SqlParameter("@PageNumber", page));
                command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                // 1. Read Users List
                var users = new List<UserViewModel>();
                while (await reader.ReadAsync())
                {
                    users.Add(new UserViewModel
                    {
                        Usercode = reader["Usercode"]?.ToString() ?? "",
                        Username = reader["Username"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Role = reader["Role"]?.ToString() ?? "",
                        StaffId = reader["StaffId"]?.ToString(),
                        Status = reader["Status"]?.ToString() ?? "active",
                        IsLicense = reader["IsLicense"]?.ToString() ?? "N",
                        CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.UtcNow,
                        LastLoginAt = reader["LastLoginAt"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginAt"]) : null,
                        RequireGpsLocation = reader["RequireGpsLocation"] != DBNull.Value && Convert.ToBoolean(reader["RequireGpsLocation"]),
                        RequireFaceScan = reader["RequireFaceScan"] != DBNull.Value && Convert.ToBoolean(reader["RequireFaceScan"]),
                        RequirePublicIp = reader["RequirePublicIp"] != DBNull.Value && Convert.ToBoolean(reader["RequirePublicIp"])
                    });
                }

                // 2. Read Total Count from second result set
                int totalCount = 0;
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                // Pass pagination details to the View via ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                ViewBag.UsercodeFilter = usercode;

                return View(users);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load user list: {ex.Message}";
                return View(new List<UserViewModel>());
            }
        }


        // GET: Add User Page
        public async Task<IActionResult> CreateUser()
        {
            var model = new UserViewModel(); // This will automatically use the '= true' defaults
            return View(model);
        }


        // GET: Edit User
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_User_List @UserID, @Usercode, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@UserID", 1)); // Replace with actual logged-in user ID if available
                command.Parameters.Add(new SqlParameter("@Usercode", id));
                command.Parameters.Add(new SqlParameter("@PageNumber", 1));
                command.Parameters.Add(new SqlParameter("@PageSize", 1));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                // Read the single user record from the first result set
                if (await reader.ReadAsync())
                {
                    var user = new UserViewModel
                    {
                        Usercode = reader["Usercode"]?.ToString() ?? "",
                        Username = reader["Username"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Role = reader["Role"]?.ToString() ?? "",
                        StaffId = reader["StaffId"]?.ToString(),
                        Status = reader["Status"]?.ToString() ?? "active",
                        IsLicense = reader["IsLicense"]?.ToString() ?? "N",
                        RequireGpsLocation = reader["RequireGpsLocation"] != DBNull.Value && Convert.ToBoolean(reader["RequireGpsLocation"]),
                        RequireFaceScan = reader["RequireFaceScan"] != DBNull.Value && Convert.ToBoolean(reader["RequireFaceScan"]),
                        RequirePublicIp = reader["RequirePublicIp"] != DBNull.Value && Convert.ToBoolean(reader["RequirePublicIp"])
                    };

                    return View("EditUser", user);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load user: {ex.Message}";
                return RedirectToAction(nameof(UserList));
            }
        }

        // POST: Add / Update User
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserViewModel model, string? password, string mode = "Add")
        {
            try
            {
                var token = new { UserId = 1, UserName = "Admin", CompanyId = 1001 };

                // Hash password if provided (blank on Update = keep existing hash)
                string? hashedPassword = null;
                if (!string.IsNullOrEmpty(password))
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var bytes = Encoding.UTF8.GetBytes(password);
                        var hash = sha256.ComputeHash(bytes);
                        hashedPassword = Convert.ToHexString(hash).ToLowerInvariant();
                    }
                }

                // Build JSON object matching the new UserViewModel fields and table schema
                var jsonBody = JsonConvert.SerializeObject(new
                {
                    Mode = mode,
                    model.Usercode,
                    model.Username,
                    model.Email,
                    model.Role,
                    model.StaffId,
                    model.Status,
                    model.IsLicense,
                    model.RequireGpsLocation,
                    model.RequireFaceScan,
                    model.RequirePublicIp,
                    PasswordHash = hashedPassword, // null on Update means "don't change password"
                    CreateBy = token.UserId,
                    UpdateBy = token.UserId
                });

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerUser @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "User"),
                        new SqlParameter("@TranType", mode),
                        new SqlParameter("@EntryPrimary", model.Usercode),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                    return Json(new { success = true, message = result.Message });
                else
                    return Json(new { success = false, message = result?.Message ?? "Error occurred." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Operation failed: {ex.Message}" });
            }
        }
    }
}