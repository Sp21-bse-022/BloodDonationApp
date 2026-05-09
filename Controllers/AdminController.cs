using BloodDonationApp.Data;
using BloodDonationApp.Models;
using BloodDonationApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationApp.Controllers
{
    /// <summary>
    /// All actions here require the "Admin" role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─── Dashboard ───────────────────────────────────────────────────────

        // GET /Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers      = await _context.Users.CountAsync();
            ViewBag.TotalDonors     = await _context.Users.CountAsync(u => u.IsAvailable);
            ViewBag.OpenRequests    = await _context.BloodRequests.CountAsync(r => r.Status == RequestStatus.Open);
            ViewBag.TotalRequests   = await _context.BloodRequests.CountAsync();
            ViewBag.TotalDonations  = await _context.DonationHistories.CountAsync();

            // Recent 10 blood requests for the dashboard table
            var recentRequests = await _context.BloodRequests
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(recentRequests);
        }

        // ─── Users ───────────────────────────────────────────────────────────

        // GET /Admin/Users
        public async Task<IActionResult> Users(string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FullName.Contains(search) ||
                    u.Email!.Contains(search) ||
                    (u.City != null && u.City.Contains(search)) ||
                    (u.BloodGroup != null && u.BloodGroup.Contains(search)));

            var users = await query
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Build view models with role info and counts
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            var adminUserIds = adminRole != null
                ? (await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync())
                    .ToHashSet()
                : new HashSet<string>();

            var requestCounts = await _context.BloodRequests
                .GroupBy(r => r.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var donationCounts = await _context.DonationHistories
                .GroupBy(d => d.DonorId)
                .Select(g => new { DonorId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DonorId, x => x.Count);

            var viewModels = users.Select(u => new AdminUserViewModel
            {
                Id               = u.Id,
                FullName         = u.FullName,
                Email            = u.Email ?? "",
                City             = u.City,
                BloodGroup       = u.BloodGroup,
                LastDonationDate = u.LastDonationDate,
                IsAvailable      = u.IsAvailable,
                IsAdmin          = adminUserIds.Contains(u.Id),
                TotalRequests    = requestCounts.GetValueOrDefault(u.Id),
                TotalDonations   = donationCounts.GetValueOrDefault(u.Id)
            }).ToList();

            ViewBag.Search = search;
            return View(viewModels);
        }

        // GET /Admin/UserDetails/id
        public async Task<IActionResult> UserDetails(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var requests = await _context.BloodRequests
                .Where(r => r.UserId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var donations = await _context.DonationHistories
                .Where(d => d.DonorId == id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewBag.User      = user;
            ViewBag.Requests  = requests;
            ViewBag.Donations = donations;
            ViewBag.IsAdmin   = await _userManager.IsInRoleAsync(user, "Admin");

            return View();
        }

        // POST /Admin/DeleteUser/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            // Prevent deleting yourself
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own admin account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Remove related data first (FK restrict means we must clean up manually)
            var requests = _context.BloodRequests.Where(r => r.UserId == id);
            _context.BloodRequests.RemoveRange(requests);

            var donations = _context.DonationHistories.Where(d => d.DonorId == id);
            _context.DonationHistories.RemoveRange(donations);

            await _context.SaveChangesAsync();
            await _userManager.DeleteAsync(user);

            TempData["Success"] = $"User '{user.FullName}' deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // POST /Admin/ToggleAvailability/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsAvailable = !user.IsAvailable;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} availability set to {(user.IsAvailable ? "Available" : "Unavailable")}.";
            return RedirectToAction(nameof(Users));
        }

        // ─── Blood Requests ──────────────────────────────────────────────────

        // GET /Admin/Requests
        public async Task<IActionResult> Requests(string? status, string? bloodGroup)
        {
            var query = _context.BloodRequests
                .Include(r => r.User)
                .AsQueryable();

            if (Enum.TryParse<RequestStatus>(status, out var statusEnum))
                query = query.Where(r => r.Status == statusEnum);

            if (!string.IsNullOrWhiteSpace(bloodGroup))
                query = query.Where(r => r.BloodGroup == bloodGroup);

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Status     = status;
            ViewBag.BloodGroup = bloodGroup;
            return View(requests);
        }

        // POST /Admin/DeleteRequest/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.BloodRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Blood request deleted.";
            return RedirectToAction(nameof(Requests));
        }

        // POST /Admin/UpdateRequestStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRequestStatus(int id, RequestStatus status)
        {
            var request = await _context.BloodRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Request status updated to {status}.";
            return RedirectToAction(nameof(Requests));
        }

        // ─── Donations ───────────────────────────────────────────────────────

        // GET /Admin/Donations
        public async Task<IActionResult> Donations()
        {
            var donations = await _context.DonationHistories
                .Include(d => d.Donor)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        // POST /Admin/DeleteDonation/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDonation(int id)
        {
            var donation = await _context.DonationHistories.FindAsync(id);
            if (donation == null) return NotFound();

            _context.DonationHistories.Remove(donation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Donation record deleted.";
            return RedirectToAction(nameof(Donations));
        }
    }
}
