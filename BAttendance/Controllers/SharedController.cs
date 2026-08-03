using BAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace BAttendance.Controllers
{
    public class SharedController : Controller
    {

        protected readonly _DbContext _context;
        protected readonly IWebHostEnvironment _env;

        public SharedController(_DbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        protected LoginResult GetTokenData()
        {
            var tokenBase64 =
                HttpContext.Session.GetString("UserToken");

            if (string.IsNullOrEmpty(tokenBase64))
                return null;

            try
            {
                var tokenJson =
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(tokenBase64));

                return JsonConvert.DeserializeObject<LoginResult>(tokenJson);
            }
            catch
            {
                return null;
            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
