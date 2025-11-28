using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballFieldBooking_New.Controllers
{
    public class FootballFieldsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FootballFieldsController> _logger;

        public FootballFieldsController(ApplicationDbContext context, ILogger<FootballFieldsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: FootballFields
        // Minh họa: ViewData, ViewBag, Query parameters
        public async Task<IActionResult> Index(string searchTerm, int? fieldTypeId)
        {
            // ViewData cho SEO và Title
            ViewData["Title"] = "Danh sách sân bóng";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentFieldType"] = fieldTypeId;

            // ViewBag cho dropdown filter
            ViewBag.FieldTypes = await _context.FieldTypes.ToListAsync();

            // Query với filtering
            var query = _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .Where(f => f.IsAvailable)
                .AsQueryable();

            // Search by name or location
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => f.Name.Contains(searchTerm) ||
                                        f.Location.Contains(searchTerm) ||
                                        f.Description.Contains(searchTerm));
            }

            // Filter by field type
            if (fieldTypeId.HasValue)
            {
                query = query.Where(f => f.FieldTypeId == fieldTypeId.Value);
            }

            var fields = await query.OrderBy(f => f.Name).ToListAsync();

            // TempData nếu không tìm thấy
            if (!fields.Any() && (!string.IsNullOrEmpty(searchTerm) || fieldTypeId.HasValue))
            {
                TempData["Info"] = "Không tìm thấy sân phù hợp. Hiển thị tất cả sân.";
                fields = await _context.FootballFields
                    .Include(f => f.FieldType)
                    .Include(f => f.TimeSlots)
                    .Where(f => f.IsAvailable)
                    .ToListAsync();
            }

            return View(fields);
        }

        // ===== FIX: Details Method - Load đầy đủ dữ liệu cho Reviews =====
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy sân!";
                return RedirectToAction(nameof(Index));
            }

            // ✅ LOAD ĐẦY ĐỦ DỮ LIỆU CHO REVIEWS
            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .Include(f => f.Reviews)                    // ← Load Reviews
                    .ThenInclude(r => r.User)               // ← Load User của Review
                .Include(f => f.Bookings)                   // ← Load Bookings để check completed
                    .ThenInclude(b => b.User)               // ← Load User của Booking
                .FirstOrDefaultAsync(m => m.Id == id);

            if (field == null)
            {
                TempData["Error"] = "Sân không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // ViewData cho breadcrumb
            ViewData["Title"] = field.Name;
            ViewData["Breadcrumb"] = "Danh sách sân / " + field.Name;

            // ViewBag cho related info
            ViewBag.MinPrice = field.TimeSlots.Any() ? field.TimeSlots.Min(ts => ts.PricePerHour) : 0;
            ViewBag.MaxPrice = field.TimeSlots.Any() ? field.TimeSlots.Max(ts => ts.PricePerHour) : 0;
            ViewBag.TotalSlots = field.TimeSlots.Count;

            // ✅ DEBUG: Log để kiểm tra dữ liệu
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var completedBookings = field.Bookings
                    .Where(b => b.UserId == userId && b.Status == BookingStatus.Completed)
                    .ToList();

                var userReview = field.Reviews.FirstOrDefault(r => r.UserId == userId);

                _logger.LogInformation("=== REVIEW DEBUG INFO ===");
                _logger.LogInformation($"Field ID: {id}");
                _logger.LogInformation($"User ID: {userId}");
                _logger.LogInformation($"Total Bookings for this field: {field.Bookings.Count(b => b.UserId == userId)}");
                _logger.LogInformation($"Completed Bookings: {completedBookings.Count}");
                _logger.LogInformation($"User has Review: {userReview != null}");
                _logger.LogInformation($"Total Reviews: {field.Reviews.Count}");
                _logger.LogInformation($"Average Rating: {field.AverageRating}");

                if (completedBookings.Any())
                {
                    foreach (var booking in completedBookings)
                    {
                        _logger.LogInformation($"  - Booking ID: {booking.Id}, PlayDate: {booking.PlayDate}, Status: {booking.Status}");
                    }
                }
                else
                {
                    _logger.LogWarning("User has NO completed bookings for this field!");
                }

                _logger.LogInformation("========================");
            }
            else
            {
                _logger.LogInformation("User is not authenticated");
            }

            return View(field);
        }

        // GET: FootballFields/GetAvailableSlots
        // Minh họa: JsonResult, AJAX endpoint
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int fieldId, DateTime date)
        {
            var slots = await _context.TimeSlots
                .Where(ts => ts.FootballFieldId == fieldId)
                .Select(ts => new
                {
                    ts.Id,
                    StartTime = ts.StartTime.ToString(@"hh\:mm"),
                    EndTime = ts.EndTime.ToString(@"hh\:mm"),
                    ts.PricePerHour,
                    ts.IsWeekend,
                    IsBooked = _context.Bookings.Any(b =>
                        b.TimeSlotId == ts.Id &&
                        b.PlayDate.Date == date.Date &&
                        b.Status != BookingStatus.Cancelled)
                })
                .ToListAsync();

            return Json(slots);
        }
    }
}