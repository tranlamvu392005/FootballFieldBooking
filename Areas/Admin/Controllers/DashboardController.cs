using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ILogger<DashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["Title"] = "Dashboard";

                // ===== BOOKING STATISTICS =====
                var totalFields = await _context.FootballFields.CountAsync();
                var totalBookings = await _context.Bookings.CountAsync();
                var todayBookings = await _context.Bookings
                    .Where(b => b.PlayDate.Date == DateTime.Today)
                    .CountAsync();
                var pendingBookings = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Pending)
                    .CountAsync();
                var totalRevenue = await _context.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;
                var todayRevenue = await _context.Bookings
                    .Where(b => b.PlayDate.Date == DateTime.Today &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed))
                    .SumAsync(b => (decimal?)b.TotalPrice) ?? 0;

                // Tạo stats object
                var stats = new
                {
                    TotalFields = totalFields,
                    TotalBookings = totalBookings,
                    TodayBookings = todayBookings,
                    PendingBookings = pendingBookings,
                    TotalRevenue = totalRevenue,
                    TodayRevenue = todayRevenue
                };
                ViewBag.Stats = stats;

                // ===== USER STATISTICS - THÊM MỚI =====
                var allUsers = _userManager.Users.ToList();
                var sevenDaysAgo = DateTime.Now.AddDays(-7);

                // Total users
                ViewBag.TotalUsers = allUsers.Count;

                // New users in last 7 days
                var newUsersCount = 0;
                foreach (var user in allUsers)
                {
                    var profile = await _context.UserProfiles
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (profile != null && profile.CreatedDate >= sevenDaysAgo)
                    {
                        newUsersCount++;
                    }
                }
                ViewBag.NewUsersCount = newUsersCount;

                // Count admins and customers
                var adminCount = 0;
                var customerCount = 0;
                var lockedCount = 0;

                foreach (var user in allUsers)
                {
                    // Check if locked
                    if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
                    {
                        lockedCount++;
                    }

                    // Check roles
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        adminCount++;
                    }
                    else if (roles.Contains("Customer"))
                    {
                        customerCount++;
                    }
                }

                ViewBag.AdminCount = adminCount;
                ViewBag.CustomerCount = customerCount;
                ViewBag.LockedUsersCount = lockedCount;

                // ===== RECENT BOOKINGS =====
                var recentBookings = await _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.FootballField)
                        .ThenInclude(f => f.FieldType)
                    .Include(b => b.TimeSlot)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(10)
                    .ToListAsync();
                ViewBag.RecentBookings = recentBookings ?? new List<Booking>();

                // ===== POPULAR FIELDS =====
                var popularFields = await _context.Bookings
                    .Include(b => b.FootballField)
                        .ThenInclude(f => f.FieldType)
                    .GroupBy(b => b.FootballField)
                    .Select(g => new
                    {
                        Field = g.Key,
                        BookingCount = g.Count()
                    })
                    .OrderByDescending(x => x.BookingCount)
                    .Take(5)
                    .ToListAsync();
                ViewBag.PopularFields = popularFields;

                _logger.LogInformation("Dashboard loaded successfully");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Có lỗi khi tải dashboard. Vui lòng thử lại!";

                // Set default values để tránh crash
                ViewBag.Stats = new
                {
                    TotalFields = 0,
                    TotalBookings = 0,
                    TodayBookings = 0,
                    PendingBookings = 0,
                    TotalRevenue = 0m,
                    TodayRevenue = 0m
                };
                ViewBag.TotalUsers = 0;
                ViewBag.NewUsersCount = 0;
                ViewBag.AdminCount = 0;
                ViewBag.CustomerCount = 0;
                ViewBag.LockedUsersCount = 0;
                ViewBag.RecentBookings = new List<Booking>();
                ViewBag.PopularFields = new List<object>();

                return View();
            }
        }
    }
}