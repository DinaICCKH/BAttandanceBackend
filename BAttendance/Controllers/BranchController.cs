using BAttendance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BAttendance.Controllers
{
    public class BranchController : SharedController
    {
        public BranchController(_DbContext context, IWebHostEnvironment env)
            : base(context, env)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? branchName = "ALL", int page = 1, int pageSize = 10)
        {
            try
            {
                branchName = string.IsNullOrEmpty(branchName) ? "ALL" : branchName;

                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_Branch_List @BranchID, @BranchName, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@BranchID", DBNull.Value)); // Pass null to retrieve list based on name filter
                command.Parameters.Add(new SqlParameter("@BranchName", branchName));
                command.Parameters.Add(new SqlParameter("@PageNumber", page));
                command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                // 1. Read Branch List
                var branches = new List<BranchViewModel>();
                while (await reader.ReadAsync())
                {
                    branches.Add(new BranchViewModel
                    {
                        Id = reader["Id"] != DBNull.Value ? Guid.Parse(reader["Id"].ToString()!) : Guid.Empty,
                        BranchName = reader["BranchName"]?.ToString() ?? "",
                        Address = reader["Address"]?.ToString(),
                        Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : 0,
                        Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : 0,
                        AllowedRadiusMeters = reader["AllowedRadiusMeters"] != DBNull.Value ? Convert.ToInt32(reader["AllowedRadiusMeters"]) : 150,
                        Status = reader["Status"]?.ToString() ?? "active",
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
                        OpeningTimeThursda = reader["OpeningTimeThursda"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeThursda"] : new TimeSpan(8, 0, 0),
                        ClosingTimeThursda = reader["ClosingTimeThursda"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeThursda"] : new TimeSpan(17, 0, 0),
                        IsFriday = reader["IsFriday"]?.ToString() ?? "Y",
                        OpeningTimeFriday = reader["OpeningTimeFriday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeFriday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeFriday = reader["ClosingTimeFriday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeFriday"] : new TimeSpan(17, 0, 0),
                        IsSaturday = reader["IsSaturday"]?.ToString() ?? "N",
                        OpeningTimeSaturday = reader["OpeningTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSaturday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSaturday = reader["ClosingTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSaturday"] : new TimeSpan(17, 0, 0),
                        IsSunday = reader["IsSunday"]?.ToString() ?? "N",
                        OpeningTimeSunday = reader["OpeningTimeSunday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSunday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSunday = reader["ClosingTimeSunday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSunday"] : new TimeSpan(17, 0, 0),
                        IsIpRestrictionEnabled = reader["IsIpRestrictionEnabled"] != DBNull.Value && Convert.ToBoolean(reader["IsIpRestrictionEnabled"]),
                        CompanyPublicIP = reader["CompanyPublicIP"]?.ToString()
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
                ViewBag.BranchNameFilter = branchName;

                return View(branches);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load branch list: {ex.Message}";
                return View(new List<BranchViewModel>());
            }
        }

        // GET: /Branch/Create
        public IActionResult Create()
        {
            var model = new BranchViewModel
            {
                AllowedRadiusMeters = 150,
                Status = "active"
            };
            return View(model);
        }

        // GET: /Branch/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            try
            {
                using var command = _context.Database.GetDbConnection().CreateCommand();
                command.CommandText = "EXEC dbo.GET_Branch_List @BranchID, @BranchName, @PageNumber, @PageSize";
                command.Parameters.Add(new SqlParameter("@BranchID", id));
                command.Parameters.Add(new SqlParameter("@BranchName", "ALL"));
                command.Parameters.Add(new SqlParameter("@PageNumber", 1));
                command.Parameters.Add(new SqlParameter("@PageSize", 1));

                await _context.Database.OpenConnectionAsync();
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var branch = new BranchViewModel
                    {
                        Id = reader["Id"] != DBNull.Value ? Guid.Parse(reader["Id"].ToString()!) : Guid.Empty,
                        BranchName = reader["BranchName"]?.ToString() ?? "",
                        Address = reader["Address"]?.ToString(),
                        Latitude = reader["Latitude"] != DBNull.Value ? Convert.ToDouble(reader["Latitude"]) : 0,
                        Longitude = reader["Longitude"] != DBNull.Value ? Convert.ToDouble(reader["Longitude"]) : 0,
                        AllowedRadiusMeters = reader["AllowedRadiusMeters"] != DBNull.Value ? Convert.ToInt32(reader["AllowedRadiusMeters"]) : 150,
                        Status = reader["Status"]?.ToString() ?? "active",
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
                        OpeningTimeThursda = reader["OpeningTimeThursda"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeThursda"] : new TimeSpan(8, 0, 0),
                        ClosingTimeThursda = reader["ClosingTimeThursda"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeThursda"] : new TimeSpan(17, 0, 0),
                        IsFriday = reader["IsFriday"]?.ToString() ?? "Y",
                        OpeningTimeFriday = reader["OpeningTimeFriday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeFriday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeFriday = reader["ClosingTimeFriday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeFriday"] : new TimeSpan(17, 0, 0),
                        IsSaturday = reader["IsSaturday"]?.ToString() ?? "N",
                        OpeningTimeSaturday = reader["OpeningTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSaturday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSaturday = reader["ClosingTimeSaturday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSaturday"] : new TimeSpan(17, 0, 0),
                        IsSunday = reader["IsSunday"]?.ToString() ?? "N",
                        OpeningTimeSunday = reader["OpeningTimeSunday"] != DBNull.Value ? (TimeSpan)reader["OpeningTimeSunday"] : new TimeSpan(8, 0, 0),
                        ClosingTimeSunday = reader["ClosingTimeSunday"] != DBNull.Value ? (TimeSpan)reader["ClosingTimeSunday"] : new TimeSpan(17, 0, 0),
                        IsIpRestrictionEnabled = reader["IsIpRestrictionEnabled"] != DBNull.Value && Convert.ToBoolean(reader["IsIpRestrictionEnabled"]),
                        CompanyPublicIP = reader["CompanyPublicIP"]?.ToString()
                    };

                    return View("Edit", branch);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to load branch: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Branch/Create or Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchViewModel model, string mode = "Add")
        {
            try
            {
                // Ensure a unique ID exists if adding new
                if (mode == "Add" && model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }

                // Serialize entire branch model to JSON string to match ControllerBranch procedure expectations
                var jsonBody = JsonConvert.SerializeObject(new
                {
                    Mode = mode,
                    model.Id,
                    model.BranchName,
                    model.Address,
                    model.Latitude,
                    model.Longitude,
                    model.AllowedRadiusMeters,
                    model.Status,
                    model.IsMonday,
                    OpeningTimeMonday = model.OpeningTimeMonday.ToString("c"),
                    ClosingTimeMonday = model.ClosingTimeMonday.ToString("c"),
                    model.IsTuesday,
                    OpeningTimeTuesday = model.OpeningTimeTuesday.ToString("c"),
                    ClosingTimeTuesday = model.ClosingTimeTuesday.ToString("c"),
                    model.IsWednesday,
                    OpeningTimeWednesday = model.OpeningTimeWednesday.ToString("c"),
                    ClosingTimeWednesday = model.ClosingTimeWednesday.ToString("c"),
                    model.IsThursday,
                    OpeningTimeThursda = model.OpeningTimeThursda.ToString("c"),
                    ClosingTimeThursda = model.ClosingTimeThursda.ToString("c"),
                    model.IsFriday,
                    OpeningTimeFriday = model.OpeningTimeFriday.ToString("c"),
                    ClosingTimeFriday = model.ClosingTimeFriday.ToString("c"),
                    model.IsSaturday,
                    OpeningTimeSaturday = model.OpeningTimeSaturday.ToString("c"),
                    ClosingTimeSaturday = model.ClosingTimeSaturday.ToString("c"),
                    model.IsSunday,
                    OpeningTimeSunday = model.OpeningTimeSunday.ToString("c"),
                    ClosingTimeSunday = model.ClosingTimeSunday.ToString("c"),
                    model.IsIpRestrictionEnabled,
                    model.CompanyPublicIP
                });

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerBranch @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "Branch"),
                        new SqlParameter("@TranType", mode),
                        new SqlParameter("@EntryPrimary", model.Id.ToString()),
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