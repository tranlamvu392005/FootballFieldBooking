using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using FootballFieldBooking_New.Services;  // ← THÊM using cho EmailService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;      // ← THÊM using cho UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballFieldBooking_New.Controllers
{
    [Authorize] // Yêu cầu đăng nhập
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;              // ← THÊM field
        private readonly UserManager<IdentityUser> _userManager;   // ← THÊM field
        private readonly ILogger<BookingsController> _logger;      // ← THÊM field (optional)

        // ===== CẬP NHẬT Constructor =====
        public BookingsController(
            ApplicationDbContext context,
            IEmailService emailService,                    // ← THÊM parameter
            UserManager<IdentityUser> userManager,         // ← THÊM parameter
            ILogger<BookingsController> logger)            // ← THÊM parameter (optional)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Bookings
        // Lịch sử đặt sân của user
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = await _context.Bookings
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            ViewData["Title"] = "Lịch sử đặt sân";
            ViewBag.PendingCount = bookings.Count(b => b.Status == BookingStatus.Pending);
            ViewBag.ConfirmedCount = bookings.Count(b => b.Status == BookingStatus.Confirmed);

            return View(bookings);
        }

        // GET: Bookings/Create
        public async Task<IActionResult> Create(int? fieldId)
        {
            if (fieldId == null)
            {
                TempData["Error"] = "Vui lòng chọn sân!";
                return RedirectToAction("Index", "FootballFields");
            }

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null)
            {
                TempData["Error"] = "Sân không tồn tại!";
                return RedirectToAction("Index", "FootballFields");
            }

            ViewData["Title"] = "Đặt sân - " + field.Name;
            ViewBag.Field = field;
            ViewBag.MinDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            return View();
        }

        // ===== CẬP NHẬT POST: Bookings/Create - THÊM GỬI EMAIL =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int fieldId, int timeSlotId, DateTime playDate, string? notes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy field info để hiển thị lại nếu có lỗi
            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null)
            {
                TempData["Error"] = "Sân không tồn tại!";
                return RedirectToAction("Index", "FootballFields");
            }

            // Validate date
            if (playDate.Date < DateTime.Today.AddDays(1))
            {
                TempData["Error"] = "Ngày chơi phải từ ngày mai trở đi!";

                ViewData["Title"] = "Đặt sân - " + field.Name;
                ViewBag.Field = field;
                ViewBag.MinDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

                return View();
            }

            // Validate time slot
            var timeSlot = await _context.TimeSlots.FindAsync(timeSlotId);
            if (timeSlot == null)
            {
                TempData["Error"] = "Khung giờ không hợp lệ!";

                ViewData["Title"] = "Đặt sân - " + field.Name;
                ViewBag.Field = field;
                ViewBag.MinDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

                return View();
            }

            // Check duplicate booking
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.FootballFieldId == fieldId &&
                              b.TimeSlotId == timeSlotId &&
                              b.PlayDate.Date == playDate.Date &&
                              b.Status != BookingStatus.Cancelled);

            if (existingBooking)
            {
                TempData["Error"] = "Khung giờ này đã được đặt! Vui lòng chọn khung giờ khác.";

                ViewData["Title"] = "Đặt sân - " + field.Name;
                ViewBag.Field = field;
                ViewBag.MinDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

                return View();
            }

            // Create booking
            var booking = new Booking
            {
                UserId = userId!,
                FootballFieldId = fieldId,
                TimeSlotId = timeSlotId,
                PlayDate = playDate,
                BookingDate = DateTime.Now,
                TotalPrice = timeSlot.PricePerHour,
                Status = BookingStatus.Pending,
                Notes = notes
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // ✅ ===== THÊM PHẦN GỬI EMAIL =====
            try
            {
                // Lấy thông tin user
                var user = await _userManager.GetUserAsync(User);

                if (user?.Email != null)
                {
                    // Format time slot string
                    var timeSlotStr = $"{timeSlot.StartTime:hh\\:mm} - {timeSlot.EndTime:hh\\:mm}";

                    // Gửi email xác nhận
                    await _emailService.SendBookingConfirmationAsync(
                        toEmail: user.Email,
                        userName: user.UserName ?? "Khách hàng",
                        bookingId: booking.Id,
                        fieldName: field.Name,
                        playDate: playDate,
                        timeSlot: timeSlotStr,
                        totalPrice: booking.TotalPrice
                    );

                    _logger.LogInformation($"✅ Confirmation email sent to {user.Email} for booking #{booking.Id}");

                    TempData["Success"] = "Đặt sân thành công! Vui lòng kiểm tra email để xem chi tiết.";
                }
                else
                {
                    _logger.LogWarning($"⚠️ User email is null, cannot send confirmation for booking #{booking.Id}");
                    TempData["Success"] = "Đặt sân thành công! Vui lòng chờ xác nhận.";
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng KHÔNG làm fail booking process
                _logger.LogError(ex, $"❌ Failed to send confirmation email for booking #{booking.Id}");

                // Vẫn hiện thông báo thành công vì booking đã được tạo
                TempData["Success"] = "Đặt sân thành công! Vui lòng chờ xác nhận.";
                TempData["Warning"] = "Không thể gửi email xác nhận. Vui lòng kiểm tra mục 'Lịch sử đặt sân'.";
            }

            return RedirectToAction("Details", new { id = booking.Id });
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (booking == null) return NotFound();

            ViewData["Title"] = "Chi tiết đặt sân #" + booking.Id;

            return View(booking);
        }

        // ===== CẬP NHẬT POST: Bookings/Cancel - THÊM GỬI EMAIL =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var booking = await _context.Bookings
                .Include(b => b.FootballField)      // ← THÊM Include để lấy tên sân cho email
                .Include(b => b.TimeSlot)           // ← THÊM Include để lấy thời gian cho email
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Completed)
            {
                TempData["Error"] = "Không thể hủy đơn đã hoàn thành!";
                return RedirectToAction("Details", new { id });
            }

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            // ✅ ===== THÊM PHẦN GỬI EMAIL HỦY ĐƠN =====
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user?.Email != null)
                {
                    await _emailService.SendBookingCancellationAsync(
                        toEmail: user.Email,
                        userName: user.UserName ?? "Khách hàng",
                        bookingId: booking.Id
                    );

                    _logger.LogInformation($"✅ Cancellation email sent to {user.Email} for booking #{booking.Id}");

                    TempData["Success"] = "Đã hủy đơn đặt sân! Email thông báo đã được gửi.";
                }
                else
                {
                    TempData["Success"] = "Đã hủy đơn đặt sân!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to send cancellation email for booking #{booking.Id}");

                // Vẫn hiện thông báo thành công vì đã hủy được booking
                TempData["Success"] = "Đã hủy đơn đặt sân!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}