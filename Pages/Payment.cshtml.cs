using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballFieldBooking_New.Pages
{
    [Authorize]
    public class PaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentModel> _logger;

        public PaymentModel(ApplicationDbContext context, ILogger<PaymentModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Properties để hiển thị
        public Booking Booking { get; set; } = null!;
        public FootballField Field { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;

        // Bind properties cho form
        [BindProperty]
        public PaymentMethod SelectedPaymentMethod { get; set; }

        [BindProperty]
        public bool AcceptTerms { get; set; }

        public string ErrorMessage { get; set; } = "";

        // GET: /Payment?bookingId=1
        public async Task<IActionResult> OnGetAsync(int? bookingId)
        {
            if (bookingId == null)
            {
                ErrorMessage = "Không tìm thấy đơn đặt sân!";
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking = await _context.Bookings
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .Include(b => b.TimeSlot)
                .Include(b => b.User)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (Booking == null)
            {
                ErrorMessage = "Đơn đặt sân không tồn tại hoặc không thuộc về bạn!";
                return Page();
            }

            // Check if already paid
            if (Booking.Payment != null && Booking.Payment.IsPaid)
            {
                return RedirectToPage("/PaymentSuccess", new { bookingId = Booking.Id });
            }

            Field = Booking.FootballField;
            TimeSlot = Booking.TimeSlot;

            return Page();
        }

        // POST: /Payment
        public async Task<IActionResult> OnPostAsync(int bookingId)
        {
            if (!AcceptTerms)
            {
                ErrorMessage = "Bạn phải đồng ý với điều khoản dịch vụ!";
                await OnGetAsync(bookingId);
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Booking = await _context.Bookings
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (Booking == null)
            {
                ErrorMessage = "Đơn đặt sân không tồn tại!";
                return Page();
            }

            // Check if already paid
            if (Booking.Payment != null && Booking.Payment.IsPaid)
            {
                return RedirectToPage("/PaymentSuccess", new { bookingId = Booking.Id });
            }

            // Create or update payment
            if (Booking.Payment == null)
            {
                var payment = new Payment
                {
                    BookingId = Booking.Id,
                    Amount = Booking.TotalPrice,
                    Method = SelectedPaymentMethod,
                    PaymentDate = DateTime.Now,
                    IsPaid = true
                };
                _context.Payments.Add(payment);
            }
            else
            {
                Booking.Payment.Method = SelectedPaymentMethod;
                Booking.Payment.PaymentDate = DateTime.Now;
                Booking.Payment.IsPaid = true;
            }

            // Update booking status
            if (Booking.Status == BookingStatus.Pending)
            {
                Booking.Status = BookingStatus.Confirmed;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Payment completed for booking #{Booking.Id} with method {SelectedPaymentMethod}");

            return RedirectToPage("/PaymentSuccess", new { bookingId = Booking.Id });
        }
    }
}