using BloodDonationApp.Data;
using BloodDonationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationApp.Controllers
{
    [Authorize]
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /Donation
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var donations = await _context.DonationHistories
                .Where(d => d.DonorId == userId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        // GET /Donation/Create
        public IActionResult Create() => View();

        // POST /Donation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationHistory model)
        {
            // DonorId is set by the server — remove from validation
            ModelState.Remove(nameof(DonationHistory.DonorId));

            if (!ModelState.IsValid)
                return View(model);

            model.DonorId = _userManager.GetUserId(User)!;
            _context.DonationHistories.Add(model);

            // Update the user's LastDonationDate and availability
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.LastDonationDate = model.DonationDate;
                // Donors must wait 56 days (8 weeks) between donations
                user.IsAvailable = (DateTime.UtcNow - model.DonationDate).TotalDays >= 56;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Donation recorded successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Donation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var donation = await _context.DonationHistories.FindAsync(id);
            if (donation == null) return NotFound();

            if (donation.DonorId != _userManager.GetUserId(User))
                return Forbid();

            _context.DonationHistories.Remove(donation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Donation record deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
