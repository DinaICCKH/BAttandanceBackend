using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using BAttendance.Models;

namespace BAttendance.Controllers
{
    [Route("License/[action]/{id?}")]
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
            var licenses = await _context.Licenses
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return View(licenses);
        }

        // POST: /License/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate()
        {
            // Manually extract and parse form values to prevent model-binding reflection mismatches
            string companyName = Request.Form["companyName"];
            string companyUniqueId = Request.Form["companyUniqueId"];
            string location = Request.Form["location"];
            string userType = Request.Form["userType"];
            string dateStr = Request.Form["expirationDate"];
            string qtyStr = Request.Form["quantity"];

            int quantity = 1;
            int.TryParse(qtyStr, out quantity);
            if (quantity <= 0) quantity = 1;

            DateTime expirationDate = DateTime.UtcNow.AddYears(1);
            if (DateTime.TryParse(dateStr, out var parsedDate))
            {
                expirationDate = parsedDate;
            }

            if (string.IsNullOrWhiteSpace(companyUniqueId) || string.IsNullOrWhiteSpace(companyName))
            {
                ModelState.AddModelError("", "Company Name and Company Unique ID are required.");
                var licenses = await _context.Licenses.OrderByDescending(l => l.CreatedAt).ToListAsync();
                return View("Index", licenses);
            }

            var generatedRawKeys = new List<string>();

            for (int i = 0; i < quantity; i++)
            {
                string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                string finalUniqueId = quantity > 1 ? $"{companyUniqueId}-{i + 1}" : companyUniqueId;

                // Automatically generate a structured raw key combining info
                string rawGeneratedKey = $"LIC-{companyName.ToUpper().Replace(" ", "")}-{userType.ToUpper()}-{uniqueSuffix}";
                generatedRawKeys.Add(rawGeneratedKey);

                string saltedHashKey = HashLicenseKey(rawGeneratedKey);

                var newLicense = new License
                {
                    CompanyName = companyName,
                    CompanyUniqueId = finalUniqueId,
                    Location = location,
                    LicenseKeyHash = saltedHashKey,
                    UserType = userType,
                    IsActive = true,
                    ExpirationDate = expirationDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Licenses.Add(newLicense);
            }

            await _context.SaveChangesAsync();

            TempData["GeneratedKeys"] = string.Join(", ", generatedRawKeys);
            TempData["SuccessMessage"] = $"Successfully generated {quantity} license(s) for {companyName}.";

            return RedirectToAction(nameof(Index));
        }

        private string HashLicenseKey(string rawKey)
        {
            string internalSalt = "BAttendance_Secure_Salt_2026_Key_Vault";
            string combinedString = rawKey + internalSalt;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}