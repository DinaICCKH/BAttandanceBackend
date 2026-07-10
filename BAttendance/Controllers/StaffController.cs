using BAttendance.Models;
using Microsoft.AspNetCore.Mvc;

namespace BAttendance.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Branch/Create
        public IActionResult Create()
        {
            var model = new StaffViewModel
            {
                Status = "active"
            };
            return View(model);
        }

        // POST: /Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // TODO: map to your Branches entity and save, e.g.
            // var branch = new Branch
            // {
            //     Id = Guid.NewGuid(),
            //     BranchName = model.BranchName,
            //     Address = model.Address,
            //     Latitude = model.Latitude,
            //     Longitude = model.Longitude,
            //     AllowedRadiusMeters = model.AllowedRadiusMeters,
            //     Status = model.Status
            // };
            // _context.Branches.Add(branch);
            // _context.SaveChanges();

            TempData["SuccessMessage"] = $"Branch \"{model.StaffCode}\" was created.";
            return RedirectToAction(nameof(Index));
        }
    }
}
