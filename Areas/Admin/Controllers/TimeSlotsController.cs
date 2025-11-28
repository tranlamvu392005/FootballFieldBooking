using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TimeSlotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeSlotsController> _logger;

        public TimeSlotsController(ApplicationDbContext context, ILogger<TimeSlotsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/TimeSlots?fieldId=1
        public async Task<IActionResult> Index(int? fieldId)
        {
            if (fieldId == null)
            {
                return RedirectToAction("Index", "Fields");
            }

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null)
            {
                TempData["Error"] = "Sân không tồn tại!";
                return RedirectToAction("Index", "Fields");
            }

            ViewData["Title"] = "Quản lý khung giờ - " + field.Name;
            ViewBag.Field = field;

            var timeSlots = await _context.TimeSlots
                .Where(ts => ts.FootballFieldId == fieldId)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            return View(timeSlots);
        }

        // GET: Admin/TimeSlots/Create?fieldId=1
        public async Task<IActionResult> Create(int? fieldId)
        {
            if (fieldId == null)
            {
                return RedirectToAction("Index", "Fields");
            }

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            if (field == null)
            {
                TempData["Error"] = "Sân không tồn tại!";
                return RedirectToAction("Index", "Fields");
            }

            ViewData["Title"] = "Thêm khung giờ - " + field.Name;
            ViewBag.Field = field;

            return View();
        }

        // POST: Admin/TimeSlots/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int fieldId, TimeSpan startTime, TimeSpan endTime,
            decimal pricePerHour, bool isWeekend)
        {
            // Remove navigation properties validation
            ModelState.Remove("FootballField");
            ModelState.Remove("Bookings");

            // Validate times
            if (endTime <= startTime)
            {
                ModelState.AddModelError("", "Giờ kết thúc phải sau giờ bắt đầu!");
            }

            // Check overlap
            var hasOverlap = await _context.TimeSlots
                .AnyAsync(ts => ts.FootballFieldId == fieldId &&
                               ((startTime >= ts.StartTime && startTime < ts.EndTime) ||
                                (endTime > ts.StartTime && endTime <= ts.EndTime) ||
                                (startTime <= ts.StartTime && endTime >= ts.EndTime)));

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Khung giờ bị trùng với khung giờ khác!");
            }

            if (ModelState.IsValid)
            {
                var timeSlot = new TimeSlot
                {
                    FootballFieldId = fieldId,
                    StartTime = startTime,
                    EndTime = endTime,
                    PricePerHour = pricePerHour,
                    IsWeekend = isWeekend
                };

                _context.TimeSlots.Add(timeSlot);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created time slot for field {fieldId}");
                TempData["Success"] = "Đã thêm khung giờ thành công!";
                return RedirectToAction(nameof(Index), new { fieldId });
            }

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .FirstOrDefaultAsync(f => f.Id == fieldId);

            ViewBag.Field = field;
            return View();
        }

        // GET: Admin/TimeSlots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var timeSlot = await _context.TimeSlots
                .Include(ts => ts.FootballField)
                    .ThenInclude(f => f.FieldType)
                .FirstOrDefaultAsync(ts => ts.Id == id);

            if (timeSlot == null) return NotFound();

            ViewData["Title"] = "Chỉnh sửa khung giờ";
            ViewBag.Field = timeSlot.FootballField;

            return View(timeSlot);
        }

        // POST: Admin/TimeSlots/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TimeSlot timeSlot)
        {
            if (id != timeSlot.Id) return NotFound();

            // Remove navigation properties validation
            ModelState.Remove("FootballField");
            ModelState.Remove("Bookings");

            // Validate times
            if (timeSlot.EndTime <= timeSlot.StartTime)
            {
                ModelState.AddModelError("", "Giờ kết thúc phải sau giờ bắt đầu!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(timeSlot);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Đã cập nhật khung giờ!";
                    return RedirectToAction(nameof(Index), new { fieldId = timeSlot.FootballFieldId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TimeSlotExists(timeSlot.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .FirstOrDefaultAsync(f => f.Id == timeSlot.FootballFieldId);

            ViewBag.Field = field;
            return View(timeSlot);
        }

        // POST: Admin/TimeSlots/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null) return NotFound();

            // Check if has bookings
            var hasBookings = await _context.Bookings.AnyAsync(b => b.TimeSlotId == id);
            if (hasBookings)
            {
                TempData["Error"] = "Không thể xóa khung giờ đã có lịch đặt!";
                return RedirectToAction(nameof(Index), new { fieldId = timeSlot.FootballFieldId });
            }

            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa khung giờ!";
            return RedirectToAction(nameof(Index), new { fieldId = timeSlot.FootballFieldId });
        }

        // POST: Admin/TimeSlots/CreateDefault?fieldId=1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDefault(int fieldId)
        {
            var field = await _context.FootballFields.FindAsync(fieldId);
            if (field == null) return NotFound();

            // Check if already has slots
            var hasSlots = await _context.TimeSlots.AnyAsync(ts => ts.FootballFieldId == fieldId);
            if (hasSlots)
            {
                TempData["Error"] = "Sân đã có khung giờ!";
                return RedirectToAction(nameof(Index), new { fieldId });
            }

            // Create default time slots
            var defaultSlots = new List<TimeSlot>
            {
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(6, 0, 0), EndTime = new TimeSpan(8, 0, 0), PricePerHour = 200000, IsWeekend = false },
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(10, 0, 0), PricePerHour = 250000, IsWeekend = false },
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(16, 0, 0), PricePerHour = 300000, IsWeekend = false },
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(18, 0, 0), PricePerHour = 350000, IsWeekend = false },
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(20, 0, 0), PricePerHour = 400000, IsWeekend = false },
                new TimeSlot { FootballFieldId = fieldId, StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(22, 0, 0), PricePerHour = 400000, IsWeekend = false }
            };

            _context.TimeSlots.AddRange(defaultSlots);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo 6 khung giờ mặc định!";
            return RedirectToAction(nameof(Index), new { fieldId });
        }

        private bool TimeSlotExists(int id)
        {
            return _context.TimeSlots.Any(e => e.Id == id);
        }
    }
}