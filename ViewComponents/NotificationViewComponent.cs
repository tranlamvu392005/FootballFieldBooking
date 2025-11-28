using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;

namespace FootballFieldBooking_New.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return View(new List<Booking>());
            }

            var now = DateTime.Now;

            // Lấy các booking sắp tới trong 7 ngày
            var upcomingBookings = await _context.Bookings
                .Include(b => b.FootballField)
                .Include(b => b.TimeSlot)
                .Where(b => b.UserId == userId &&
                           b.Status == BookingStatus.Confirmed &&
                           b.PlayDate >= now &&
                           b.PlayDate <= now.AddDays(7))
                .OrderBy(b => b.PlayDate)
                .Take(5)
                .ToListAsync();

            return View(upcomingBookings);
        }
    }
}
