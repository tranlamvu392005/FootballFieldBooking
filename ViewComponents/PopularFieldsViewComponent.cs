using FootballFieldBooking_New.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.ViewComponents
{
    public class PopularFieldsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public PopularFieldsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 3)
        {
            // Lấy các sân phổ biến dựa trên số lượt đặt
            var popularFields = await _context.Bookings
                .Where(b => b.FootballField != null)
                .GroupBy(b => new
                {
                    b.FootballFieldId,
                    b.FootballField.Name,
                    b.FootballField.ImageUrl,
                    b.FootballField.Location,
                    FieldTypeName = b.FootballField.FieldType.Name
                })
                .Select(g => new
                {
                    g.Key.FootballFieldId,
                    g.Key.Name,
                    g.Key.ImageUrl,
                    g.Key.Location,
                    g.Key.FieldTypeName,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(count)
                .ToListAsync();

            return View(popularFields);
        }
    }
}