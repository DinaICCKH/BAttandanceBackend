using Microsoft.AspNetCore.Mvc;
using BAttendance.Models;

namespace BAttendance.Controllers
{
    public class BranchController : Controller
    {
        // TODO: inject your DbContext here, e.g.
        // private readonly ApplicationDbContext _context;
        // public BranchController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            return View();
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

        // POST: /Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchViewModel model)
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

            TempData["SuccessMessage"] = $"Branch \"{model.BranchName}\" was created.";
            return RedirectToAction(nameof(Index));
        }
    }
}
