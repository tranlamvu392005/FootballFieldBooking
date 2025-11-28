using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FootballFieldBooking_New.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ===== ACTION METHOD - GET =====
        // Minh họa: ViewData, ViewBag, Model
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            // Lấy 6 sân nổi bật
            var featuredFields = await _context.FootballFields
                .Include(f => f.FieldType)
                .Where(f => f.IsAvailable)
                .Take(6)
                .ToListAsync();

            // ===== SỬ DỤNG ViewData =====
            ViewData["PageTitle"] = "Trang chủ - Đặt sân bóng";
            ViewData["TotalFields"] = await _context.FootballFields.CountAsync();
            ViewData["TotalBookings"] = await _context.Bookings.CountAsync();

            // ===== SỬ DỤNG ViewBag =====
            ViewBag.WelcomeMessage = "Chào mừng đến với hệ thống đặt sân bóng!";
            ViewBag.FieldTypes = await _context.FieldTypes.ToListAsync();

            // ===== LẤY REVIEWS TỪ DATABASE =====
            var topReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.FootballField)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedDate)
                .Take(3)
                .ToListAsync();

            // Tính rating trung bình
            var averageRating = await _context.Reviews.AnyAsync()
                ? await _context.Reviews.AverageAsync(r => (double)r.Rating)
                : 0;

            var totalReviews = await _context.Reviews.CountAsync();

            ViewBag.TopReviews = topReviews;
            ViewBag.AverageRating = Math.Round(averageRating, 1);
            ViewBag.TotalReviews = totalReviews;

            // ===== SỬ DỤNG TempData (sẽ dùng sau khi đặt sân) =====
            if (TempData["BookingSuccess"] != null)
            {
                ViewBag.SuccessMessage = TempData["BookingSuccess"];
            }

            // ===== RETURN Strongly Typed View =====
            return View(featuredFields);
        }

        // ===== ACTION METHOD - About =====
        public IActionResult About()
        {
            // Minh họa: ViewData cho SEO
            ViewData["Title"] = "Giới thiệu";
            ViewData["MetaDescription"] = "Hệ thống đặt sân bóng đá trực tuyến hàng đầu Việt Nam";

            return View();
        }

        // ===== ACTION METHOD - Contact =====
        public IActionResult Contact()
        {
            ViewData["Title"] = "Liên hệ";

            // ViewBag cho thông tin liên hệ
            ViewBag.Phone = "1900-xxxx";
            ViewBag.Email = "info@footballbooking.com";
            ViewBag.Address = "123 Đường ABC, Quận 1, TP.HCM";

            return View();
        }

        // ===== ACTION METHOD - POST Example =====
        [HttpPost]
        public IActionResult Subscribe(string email)
        {
            // Model Binding - email được bind tự động từ form

            if (string.IsNullOrEmpty(email))
            {
                // TempData - persist qua redirect
                TempData["Error"] = "Vui lòng nhập email!";
                return RedirectToAction("Index");
            }

            // Giả lập lưu subscription
            TempData["Success"] = $"Đã đăng ký nhận tin với email: {email}";

            // RedirectToActionResult
            return RedirectToAction("Index");
        }

        // ===== ACTION METHOD - JSON Result =====
        [HttpGet]
        public async Task<IActionResult> GetFieldStats()
        {
            // Minh họa: JsonResult
            var stats = new
            {
                TotalFields = await _context.FootballFields.CountAsync(),
                AvailableFields = await _context.FootballFields.Where(f => f.IsAvailable).CountAsync(),
                TotalBookings = await _context.Bookings.CountAsync(),
                FieldTypes = await _context.FieldTypes.Select(ft => new
                {
                    ft.Name,
                    Count = ft.FootballFields.Count
                }).ToListAsync()
            };

            return Json(stats);
        }

        // ===== ERROR HANDLING =====
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ===== PRIVACY =====
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult TagHelpersDemo()
        {
            ViewData["Title"] = "Tag Helpers Demo";
            return View();
        }
    }
}