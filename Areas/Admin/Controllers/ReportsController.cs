using FootballFieldBooking_New.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            //  1️⃣ Tổng doanh thu (đã thanh toán)
            var totalRevenue = await _context.Payments
                .Where(p => p.IsPaid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            //  2️⃣ Doanh thu trong tháng hiện tại
            var monthlyRevenue = await _context.Payments
                .Where(p => p.IsPaid && p.PaymentDate >= startOfMonth)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // 3️⃣ Tổng số lượt đặt sân
            var totalBookings = await _context.Bookings.CountAsync();

            // 4️⃣ Số lượt đặt đang chờ xác nhận
            var pendingBookings = await _context.Bookings
                .CountAsync(b => b.Status == Models.BookingStatus.Pending);

            //  5️⃣ Số khách hàng mới trong tháng
            var newCustomers = await _context.UserProfiles
                .CountAsync(u => u.CreatedDate >= startOfMonth);

            //  6️⃣ Sân được đặt nhiều nhất
            var topField = await _context.FootballFields
                .Include(f => f.Bookings)
                .OrderByDescending(f => f.Bookings.Count)
                .Select(f => f.Name)
                .FirstOrDefaultAsync() ?? "Chưa có dữ liệu";

            //  7️⃣ Dữ liệu biểu đồ doanh thu 12 tháng gần nhất
            var months = Enumerable.Range(1, 12).ToList();
            var monthRevenue = new List<decimal>();

            foreach (var m in months)
            {
                var start = new DateTime(now.Year, m, 1);
                var end = start.AddMonths(1);
                var revenue = await _context.Payments
                    .Where(p => p.IsPaid && p.PaymentDate >= start && p.PaymentDate < end)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;
                monthRevenue.Add(revenue);
            }

            // 🧾8️⃣ Tỷ lệ đặt sân theo loại sân (đã fix lỗi aggregate)
            var fieldTypeStats = await _context.Bookings
                .Include(b => b.FootballField)
                    .ThenInclude(f => f.FieldType)
                .GroupBy(b => b.FootballField.FieldType.Name)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            //  Gửi dữ liệu sang View
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.TotalBookings = totalBookings;
            ViewBag.PendingBookings = pendingBookings;
            ViewBag.NewCustomers = newCustomers;
            ViewBag.TopField = topField;

            ViewBag.MonthLabels = months.Select(m => $"Tháng {m}").ToList();
            ViewBag.MonthRevenue = monthRevenue;
            ViewBag.FieldTypeLabels = fieldTypeStats.Select(x => x.Type).ToList();
            ViewBag.FieldTypeCounts = fieldTypeStats.Select(x => x.Total).ToList();

            return View();
        }
    }
}
