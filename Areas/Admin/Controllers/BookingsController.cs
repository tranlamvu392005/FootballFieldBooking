using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Bookings
        public async Task<IActionResult> Index(string searchTerm, BookingStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            ViewData["Title"] = "Quản lý đặt sân";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentStatus"] = status;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            var query = _context.Bookings
                .Include(b => b.User)
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b => b.User.UserName.Contains(searchTerm) ||
                                        b.User.Email.Contains(searchTerm) ||
                                        b.FootballField.Name.Contains(searchTerm) ||
                                        b.Id.ToString().Contains(searchTerm));
            }

            // Filter by status
            if (status.HasValue)
            {
                query = query.Where(b => b.Status == status.Value);
            }

            // Filter by date range
            if (fromDate.HasValue)
            {
                query = query.Where(b => b.PlayDate.Date >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(b => b.PlayDate.Date <= toDate.Value.Date);
            }

            var bookings = await query.OrderByDescending(b => b.BookingDate).ToListAsync();

            // Statistics
            ViewBag.TotalBookings = bookings.Count;
            ViewBag.PendingCount = bookings.Count(b => b.Status == BookingStatus.Pending);
            ViewBag.ConfirmedCount = bookings.Count(b => b.Status == BookingStatus.Confirmed);
            ViewBag.TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                                          .Sum(b => (decimal?)b.TotalPrice) ?? 0;

            return View(bookings);
        }

        // GET: Admin/Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (booking == null) return NotFound();

            ViewData["Title"] = "Chi tiết đơn đặt #" + booking.Id;

            return View(booking);
        }

        // POST: Admin/Bookings/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Pending)
            {
                TempData["Error"] = "Chỉ có thể xác nhận đơn đang chờ!";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = BookingStatus.Confirmed;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Booking #{id} confirmed by admin");
            TempData["Success"] = $"Đã xác nhận đơn đặt #{id}!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Bookings/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Completed)
            {
                TempData["Error"] = "Không thể hủy đơn đã hoàn thành!";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = BookingStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
            {
                booking.Notes = (booking.Notes ?? "") + $"\n[Admin hủy: {reason}]";
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Booking #{id} cancelled by admin. Reason: {reason}");
            TempData["Success"] = $"Đã hủy đơn đặt #{id}!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Bookings/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound();

            if (booking.Status != BookingStatus.Confirmed)
            {
                TempData["Error"] = "Chỉ có thể hoàn thành đơn đã xác nhận!";
                return RedirectToAction(nameof(Details), new { id });
            }

            booking.Status = BookingStatus.Completed;

            // Create payment if not exists
            if (booking.Payment == null)
            {
                var payment = new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    Method = PaymentMethod.Cash,
                    PaymentDate = DateTime.Now,
                    IsPaid = true
                };
                _context.Payments.Add(payment);
            }
            else
            {
                booking.Payment.IsPaid = true;
                booking.Payment.PaymentDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Booking #{id} completed by admin");
            TempData["Success"] = $"Đã hoàn thành đơn đặt #{id}!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Bookings/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            if (booking.Status == BookingStatus.Completed)
            {
                TempData["Error"] = "Không thể xóa đơn đã hoàn thành!";
                return RedirectToAction(nameof(Index));
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Booking #{id} deleted by admin");
            TempData["Success"] = $"Đã xóa đơn đặt #{id}!";
            return RedirectToAction(nameof(Index));
        }
    }
}