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
    public class FieldsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FieldsController> _logger;

        public FieldsController(ApplicationDbContext context, ILogger<FieldsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Fields
        public async Task<IActionResult> Index(string searchTerm, int? fieldTypeId, bool? isAvailable)
        {
            ViewData["Title"] = "Quản lý sân bóng";
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentFieldType"] = fieldTypeId;
            ViewData["CurrentAvailable"] = isAvailable;

            // Load field types cho filter
            ViewBag.FieldTypes = await _context.FieldTypes.ToListAsync();

            var query = _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => f.Name.Contains(searchTerm) ||
                                        f.Location.Contains(searchTerm));
            }

            // Filter by field type
            if (fieldTypeId.HasValue)
            {
                query = query.Where(f => f.FieldTypeId == fieldTypeId.Value);
            }

            // Filter by availability
            if (isAvailable.HasValue)
            {
                query = query.Where(f => f.IsAvailable == isAvailable.Value);
            }

            var fields = await query.OrderBy(f => f.Name).ToListAsync();

            return View(fields);
        }

        // GET: Admin/Fields/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .Include(f => f.TimeSlots)
                .Include(f => f.Bookings)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (field == null) return NotFound();

            ViewData["Title"] = "Chi tiết sân - " + field.Name;

            // Statistics
            ViewBag.TotalBookings = field.Bookings.Count;
            ViewBag.CompletedBookings = field.Bookings.Count(b => b.Status == BookingStatus.Completed);
            ViewBag.Revenue = field.Bookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                .Sum(b => (decimal?)b.TotalPrice) ?? 0;

            return View(field);
        }

        // GET: Admin/Fields/Create
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm sân mới";
            ViewData["FieldTypeId"] = new SelectList(await _context.FieldTypes.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Admin/Fields/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FootballField field)
        {
            // Remove validation cho Navigation Properties
            ModelState.Remove("FieldType");
            ModelState.Remove("TimeSlots");
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default values
                    if (string.IsNullOrEmpty(field.ImageUrl))
                    {
                        field.ImageUrl = "/images/default-field.jpg";
                    }

                    _context.Add(field);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Created new field: {field.Name}");
                    TempData["Success"] = $"Đã thêm sân {field.Name} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating field");
                    ModelState.AddModelError("", "Có lỗi khi thêm sân: " + ex.Message);
                }
            }
            else
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
            }

            ViewData["Title"] = "Thêm sân mới";
            ViewData["FieldTypeId"] = new SelectList(await _context.FieldTypes.ToListAsync(), "Id", "Name", field.FieldTypeId);
            return View(field);
        }

        // GET: Admin/Fields/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var field = await _context.FootballFields.FindAsync(id);
            if (field == null) return NotFound();

            ViewData["Title"] = "Chỉnh sửa sân - " + field.Name;
            ViewData["FieldTypeId"] = new SelectList(await _context.FieldTypes.ToListAsync(), "Id", "Name", field.FieldTypeId);
            return View(field);
        }

        // POST: Admin/Fields/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FootballField field)
        {
            if (id != field.Id) return NotFound();

            // Remove validation cho Navigation Properties
            ModelState.Remove("FieldType");
            ModelState.Remove("TimeSlots");
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                try
                {
                    // Set default image if empty
                    if (string.IsNullOrEmpty(field.ImageUrl))
                    {
                        field.ImageUrl = "/images/default-field.jpg";
                    }

                    _context.Update(field);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Updated field: {field.Name}");
                    TempData["Success"] = $"Đã cập nhật sân {field.Name} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FieldExists(field.Id))
                    {
                        TempData["Error"] = "Sân không tồn tại!";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating field");
                        ModelState.AddModelError("", "Có lỗi đồng thời. Vui lòng thử lại.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating field");
                    ModelState.AddModelError("", "Có lỗi khi cập nhật: " + ex.Message);
                }
            }
            else
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
            }

            ViewData["Title"] = "Chỉnh sửa sân - " + field.Name;
            ViewData["FieldTypeId"] = new SelectList(await _context.FieldTypes.ToListAsync(), "Id", "Name", field.FieldTypeId);
            return View(field);
        }

        // POST: Admin/Fields/ToggleAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var field = await _context.FootballFields.FindAsync(id);
            if (field == null) return NotFound();

            field.IsAvailable = !field.IsAvailable;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã {(field.IsAvailable ? "kích hoạt" : "tạm ngưng")} sân {field.Name}!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Fields/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var field = await _context.FootballFields
                .Include(f => f.FieldType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (field == null) return NotFound();

            ViewData["Title"] = "Xóa sân - " + field.Name;
            return View(field);
        }

        // POST: Admin/Fields/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var field = await _context.FootballFields.FindAsync(id);
            if (field != null)
            {
                // Check if has bookings
                var hasBookings = await _context.Bookings.AnyAsync(b => b.FootballFieldId == id);
                if (hasBookings)
                {
                    TempData["Error"] = "Không thể xóa sân đã có lịch đặt. Hãy tạm ngưng sân thay vì xóa.";
                    return RedirectToAction(nameof(Index));
                }

                _context.FootballFields.Remove(field);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã xóa sân {field.Name}!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FieldExists(int id)
        {
            return _context.FootballFields.Any(e => e.Id == id);
        }
    }
}