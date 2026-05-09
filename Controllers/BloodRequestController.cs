using BloodDonationApp.Data;
using BloodDonationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationApp.Controllers
{
    public class BloodRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BloodRequestController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /BloodRequest
        public async Task<IActionResult> Index(string? bloodGroup, string? city, string? urgency)
        {
            var query = _context.BloodRequests
                .Where(r => r.Status == RequestStatus.Open)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(bloodGroup))
                query = query.Where(r => r.BloodGroup == bloodGroup);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(r => r.City.Contains(city));

            if (Enum.TryParse<UrgencyLevel>(urgency, out var urgencyEnum))
                query = query.Where(r => r.UrgencyLevel == urgencyEnum);

            var requests = await query
                .OrderByDescending(r => r.UrgencyLevel)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.BloodGroup = bloodGroup;
            ViewBag.City = city;
            ViewBag.Urgency = urgency;

            return View(requests);
        }

        // GET /BloodRequest/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.BloodRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        // GET /BloodRequest/Create
        [Authorize]
        public IActionResult Create() => View();

        // POST /BloodRequest/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BloodRequest model)
        {
            // UserId is set by the server — remove it from validation so
            // ModelState doesn't fail on a missing form field.
            ModelState.Remove(nameof(BloodRequest.UserId));

            if (!ModelState.IsValid)
                return View(model);

            model.UserId = _userManager.GetUserId(User)!;
            model.CreatedAt = DateTime.UtcNow;
            model.Status = RequestStatus.Open;

            _context.BloodRequests.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Blood request created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET /BloodRequest/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            // Only the owner can edit
            if (request.UserId != _userManager.GetUserId(User))
                return Forbid();

            return View(request);
        }

        // POST /BloodRequest/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BloodRequest model)
        {
            if (id != model.Id) return BadRequest();

            var existing = await _context.BloodRequests.FindAsync(id);
            if (existing == null) return NotFound();

            if (existing.UserId != _userManager.GetUserId(User))
                return Forbid();

            // UserId comes from the DB, not the form
            ModelState.Remove(nameof(BloodRequest.UserId));

            if (!ModelState.IsValid)
                return View(model);

            existing.PatientName = model.PatientName;
            existing.BloodGroup = model.BloodGroup;
            existing.Hospital = model.Hospital;
            existing.City = model.City;
            existing.UnitsNeeded = model.UnitsNeeded;
            existing.UrgencyLevel = model.UrgencyLevel;
            existing.ContactNumber = model.ContactNumber;
            existing.Status = model.Status;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Blood request updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST /BloodRequest/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.UserId != _userManager.GetUserId(User))
                return Forbid();

            _context.BloodRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Blood request deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
