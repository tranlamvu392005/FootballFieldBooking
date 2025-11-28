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
    public class PaymentSuccessModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PaymentSuccessModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Booking Booking { get; set; } = null!;
        public Payment Payment { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? bookingId)
        {
            if (bookingId == null)
            {
                return RedirectToPage("/Index");
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
                return NotFound();
            }

            if (Booking.Payment == null)
            {
                return RedirectToPage("/Payment", new { bookingId = Booking.Id });
            }

            Payment = Booking.Payment;

            return Page();
        }
    }
}