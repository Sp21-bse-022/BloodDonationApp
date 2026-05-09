using BloodDonationApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BloodDonationApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Pass summary stats to the home page
            ViewBag.TotalDonors = await _context.Users.CountAsync(u => u.IsAvailable);
            ViewBag.OpenRequests = await _context.BloodRequests
                .CountAsync(r => r.Status == Models.RequestStatus.Open);
            ViewBag.TotalDonations = await _context.DonationHistories.CountAsync();

            // Latest 5 open blood requests for the dashboard
            var recentRequests = await _context.BloodRequests
                .Where(r => r.Status == Models.RequestStatus.Open)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentRequests);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
