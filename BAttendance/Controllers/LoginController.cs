using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using BAttendance.Models;

namespace BAttendance.Controllers
{
    public class LoginController : Controller
    {
        private readonly _DbContext _context;
        private readonly IWebHostEnvironment _env;

        public LoginController(_DbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password, bool rememberMe)
        {
            // 1. Hash the password
            string hashedPassword = HashPassword(password);

            // 2. Call stored procedure
            var resultList = _context.LoginResults
                .FromSqlRaw(
                    "EXEC GET_login @Username, @Password",
                    new SqlParameter("@Username", username),
                    new SqlParameter("@Password", hashedPassword)
                )
                .AsNoTracking()
                .ToList();

            var result = resultList.FirstOrDefault();

            // 3. Login success
            if (result != null && result.Code == 200)
            {
                // Generate token from result
                var token = GenerateToken(result);

                // Save token in session
                HttpContext.Session.SetString("UserToken", token);

                return RedirectToAction("Index", "Home");
            }

            // 4. Login failed
            ViewBag.Error = result?.Message ?? "Login failed";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session
            return RedirectToAction("Index", "Login");
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
    }
}
