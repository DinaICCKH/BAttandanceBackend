using BAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BAttendance.Controllers
{
    public class LicenseController : SharedController
    {
        public LicenseController(_DbContext context, IWebHostEnvironment env)
           : base(context, env)
        {
        }

        // GET: /License/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var licenses = await _context.Set<License>()
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(licenses);
        }

        // POST: /License/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string companyName, string companyUniqueId, string location, string userType, int quantity, DateTime expirationDate)
        {
            if (string.IsNullOrWhiteSpace(companyName) || string.IsNullOrWhiteSpace(companyUniqueId) || quantity <= 0)
            {
                ModelState.AddModelError("", "Please fill in all required fields correctly.");
                var list = await _context.Set<License>().OrderByDescending(l => l.CreatedAt).ToListAsync();
                return View("Index", list);
            }

            var rawKeysList = new List<string>();
            var licensesToAdd = new List<License>();

            for (int i = 0; i < quantity; i++)
            {
                string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                string rawKey = $"{companyUniqueId}-{uniqueSuffix}-{DateTime.UtcNow.Ticks}";
                rawKeysList.Add(rawKey);

                string hashedKey = HashLicenseKey(rawKey);

                var licenseModel = new License
                {
                    Mode = "Add",
                    CompanyName = companyName,
                    CompanyUniqueId = $"{companyUniqueId}-{(i + 1):D2}",
                    Location = location,
                    LicenseKeyHash = hashedKey,
                    UserType = userType ?? "User",
                    IsActive = true,
                    ExpirationDate = expirationDate,
                    CreatedAt = DateTime.UtcNow
                };

                licensesToAdd.Add(licenseModel);
            }

            try
            {
                string jsonBody = JsonSerializer.Serialize(licensesToAdd);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerLicense @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "License"),
                        new SqlParameter("@TranType", "Add"),
                        new SqlParameter("@EntryPrimary", companyUniqueId),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    TempData["SuccessMessage"] = $"{quantity} license(s) generated successfully!";
                    TempData["GeneratedKeys"] = string.Join("\n", rawKeysList);
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "An error occurred while generating licenses.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Operation failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /License/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int licenseId)
        {
            try
            {
                var deleteModel = new List<License>
                {
                    new License
                    {
                        Mode = "Delete",
                        LicenseId = licenseId
                    }
                };

                string jsonBody = JsonSerializer.Serialize(deleteModel);

                var spResults = await _context.Set<SpResult>()
                    .FromSqlRaw("EXEC dbo.ControllerLicense @MasterType, @TranType, @EntryPrimary, @JsonBody",
                        new SqlParameter("@MasterType", "License"),
                        new SqlParameter("@TranType", "Delete"),
                        new SqlParameter("@EntryPrimary", licenseId.ToString()),
                        new SqlParameter("@JsonBody", jsonBody))
                    .AsNoTracking()
                    .ToListAsync();

                var result = spResults.FirstOrDefault();

                if (result != null && result.Code == 200)
                {
                    TempData["SuccessMessage"] = "License deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = result?.Message ?? "Failed to delete license.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Operation failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private string HashLicenseKey(string key)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}