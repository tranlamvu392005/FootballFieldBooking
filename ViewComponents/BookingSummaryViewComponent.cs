using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballFieldBooking_New.ViewComponents
{
    public class BookingSummaryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public BookingSummaryViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return View(new
                {
                    TotalBookings = 0,
                    PendingBookings = 0,
                    UpcomingBookings = 0
                });
            }

            var now = DateTime.Now;

            var summary = new
            {
                TotalBookings = await _context.Bookings
                    .Where(b => b.UserId == userId)
                    .CountAsync(),

                PendingBookings = await _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == BookingStatus.Pending)
                    .CountAsync(),

                UpcomingBookings = await _context.Bookings
                    .Where(b => b.UserId == userId &&
                               b.Status == BookingStatus.Confirmed &&
                               b.PlayDate > now)
                    .CountAsync()
            };

            return View(summary);
        }
    }
}