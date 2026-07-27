using BAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BAttendance.Controllers
{
    public class StaffController : SharedController
    {
        public StaffController(_DbContext context, IWebHostEnvironment env)
            : base(context, env)
        {
        }

        // GET: /Staff/Index
        public async Task<IActionResult> Index(string searchText = "ALL", int pageNumber = 1, int pageSize = 10)
        {
            var staffList = new List<StaffViewModel>();
            int totalCount = 0;

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_Staff_List @StaffId, @SearchText, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@StaffId", DBNull.Value));
                command.Parameters.Add(new SqlParameter("@SearchText", string.IsNullOrEmpty(searchText) ? "ALL" : searchText));
                command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
                command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    staffList.Add(new StaffViewModel
                    {
                        Id = reader["Id"] != DBNull.Value ? reader.GetGuid(reader.GetOrdinal("Id")) : Guid.Empty,
                        StaffCode = reader["StaffCode"]?.ToString() ?? "",
                        FullName = reader["FullName"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Phone = reader["Phone"]?.ToString() ?? "",
                        Department = reader["Department"]?.ToString(),
                        JobTitle = reader["JobTitle"]?.ToString(),
                        Status = reader["Status"]?.ToString() ?? "active",
                        Role = reader["Role"]?.ToString() ?? "Staff"
                    });
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load staff list: {ex.Message}";
            }

            ViewBag.CurrentSearch = searchText;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(staffList);
        }

        // GET: /Staff/Create
        public async Task<IActionResult> Create()
        {
            var model = new StaffViewModel
            {
                Status = "active",
                Role = "Staff",
                IsMonday = "Y",
                IsTuesday = "Y",
                IsWednesday = "Y",
                IsThursday = "Y",
                IsFriday = "Y",
                IsSaturday = "N",
                IsSunday = "N"
            };

            await PopulateUserListBagAsync();
            return View(model);
        }

        // POST: /Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                await PopulateUserListBagAsync();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                model.Mode = "Add";
                string jsonBody = JsonSerializer.Serialize(model);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerStaff @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "Staff"),
                        new SqlParameter("@TranType", "Add"),
                        new SqlParameter("@EntryPrimary", DBNull.Value),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    return Json(new { success = true, message = $"Staff \"{model.FullName}\" was created successfully." });
                }
                else
                {
                    return Json(new { success = false, message = result?.Message ?? "An error occurred." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Operation failed: {ex.Message}" });
            }
        }

        // GET: /Staff/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            StaffViewModel model = null;

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_Staff_List @StaffId, @SearchText, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@StaffId", id));
                command.Parameters.Add(new SqlParameter("@SearchText", "ALL"));
                command.Parameters.Add(new SqlParameter("@PageNumber", 1));
                command.Parameters.Add(new SqlParameter("@PageSize", 1));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    model = new StaffViewModel
                    {
                        Mode = "Update",
                        Id = reader.GetGuid(reader.GetOrdinal("Id")),
                        StaffCode = reader["StaffCode"]?.ToString() ?? "",
                        FullName = reader["FullName"]?.ToString() ?? "",
                        NationalId = reader["NationalId"]?.ToString(),
                        JoinDate = reader["JoinDate"] != DBNull.Value ? reader.GetDateTime(reader.GetOrdinal("JoinDate")) : null,
                        Email = reader["Email"]?.ToString() ?? "",
                        Phone = reader["Phone"]?.ToString() ?? "",
                        Department = reader["Department"]?.ToString(),
                        JobTitle = reader["JobTitle"]?.ToString(),
                        HomeBranchId = reader["HomeBranchId"] != DBNull.Value ? reader.GetGuid(reader.GetOrdinal("HomeBranchId")) : null,
                        IsCustomScheduleEnabled = reader["IsCustomScheduleEnabled"] != DBNull.Value && reader.GetBoolean(reader.GetOrdinal("IsCustomScheduleEnabled")),

                        IsMonday = reader["IsMonday"]?.ToString() ?? "Y",
                        OpeningTimeMonday = reader["OpeningTimeMonday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeMonday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeMonday = reader["ClosingTimeMonday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeMonday"] : new TimeSpan(17, 0, 0),

                        IsTuesday = reader["IsTuesday"]?.ToString() ?? "Y",
                        OpeningTimeTuesday = reader["OpeningTimeTuesday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeTuesday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeTuesday = reader["ClosingTimeTuesday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeTuesday"] : new TimeSpan(17, 0, 0),

                        IsWednesday = reader["IsWednesday"]?.ToString() ?? "Y",
                        OpeningTimeWednesday = reader["OpeningTimeWednesday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeWednesday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeWednesday = reader["ClosingTimeWednesday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeWednesday"] : new TimeSpan(17, 0, 0),

                        IsThursday = reader["IsThursday"]?.ToString() ?? "Y",
                        OpeningTimeThursday = reader["OpeningTimeThursday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeThursday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeThursday = reader["ClosingTimeThursday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeThursday"] : new TimeSpan(17, 0, 0),

                        IsFriday = reader["IsFriday"]?.ToString() ?? "Y",
                        OpeningTimeFriday = reader["OpeningTimeFriday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeFriday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeFriday = reader["ClosingTimeFriday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeFriday"] : new TimeSpan(17, 0, 0),

                        IsSaturday = reader["IsSaturday"]?.ToString() ?? "N",
                        OpeningTimeSaturday = reader["OpeningTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSaturday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSaturday = reader["ClosingTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSaturday"] : new TimeSpan(17, 0, 0),

                        IsSunday = reader["IsSunday"]?.ToString() ?? "N",
                        OpeningTimeSunday = reader["OpeningTimeSunday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSunday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSunday = reader["ClosingTimeSunday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSunday"] : new TimeSpan(17, 0, 0),

                        UserId = reader["UserId"]?.ToString(),
                        Role = reader["Role"]?.ToString() ?? "Staff",
                        Status = reader["Status"]?.ToString() ?? "active"
                    };
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load staff details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }

            if (model == null)
            {
                return NotFound();
            }

            await PopulateUserListBagAsync();
            return View(model);
        }

        // POST: /Staff/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, StaffViewModel model)
        {
            if (id != model.Id)
            {
                return Json(new { success = false, message = "Invalid ID mismatch." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                await PopulateUserListBagAsync();
                return Json(new { success = false, message = string.Join(" | ", errors) });
            }

            try
            {
                model.Mode = "Update";
                string jsonBody = JsonSerializer.Serialize(model);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerStaff @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "Staff"),
                        new SqlParameter("@TranType", "Update"),
                        new SqlParameter("@EntryPrimary", model.Id.ToString()),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    return Json(new { success = true, message = $"Staff \"{model.FullName}\" was updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = result?.Message ?? "An error occurred." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Operation failed: {ex.Message}" });
            }
        }

        private async Task PopulateUserListBagAsync()
        {
            var userList = new List<object>();

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_User_List @UserID, @Usercode, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@UserID", DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Usercode", "ALL"));
                command.Parameters.Add(new SqlParameter("@PageNumber", 1));
                command.Parameters.Add(new SqlParameter("@PageSize", 999999)); // Large page size to fetch all records without pagination limits

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                // 1. Read Users List
                while (await reader.ReadAsync())
                {
                    userList.Add(new
                    {
                        Id = reader["Id"]?.ToString(),
                        UserName = reader["Username"]?.ToString() ?? reader["Email"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                // Log or handle error quietly for dropdown populating
                System.Diagnostics.Debug.WriteLine($"Failed to load user dropdown list: {ex.Message}");
            }

            ViewBag.UserList = userList;
        }
    }
}