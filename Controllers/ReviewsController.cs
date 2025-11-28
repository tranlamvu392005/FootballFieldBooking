using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballFieldBooking_New.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(ApplicationDbContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int fieldId, int rating, string comment, int? bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validation
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Rating phải từ 1 đến 5 sao!";
                return RedirectToAction("Details", "FootballFields", new { id = fieldId });
            }

            if (string.IsNullOrWhiteSpace(comment) || comment.Length < 10)
            {
                TempData["Error"] = "Nội dung đánh giá phải có ít nhất 10 ký tự!";
                return RedirectToAction("Details", "FootballFields", new { id = fieldId });
            }

            // Check if user already reviewed
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.FootballFieldId == fieldId);

            if (existingReview != null)
            {
                TempData["Error"] = "Bạn đã đánh giá sân này rồi!";
                return RedirectToAction("Details", "FootballFields", new { id = fieldId });
            }

            // Check if user has completed booking (optional but recommended)
            var hasCompletedBooking = await _context.Bookings
                .AnyAsync(b => b.UserId == userId &&
                              b.FootballFieldId == fieldId &&
                              b.Status == BookingStatus.Completed);

            if (!hasCompletedBooking)
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sau khi hoàn thành đặt sân!";
                return RedirectToAction("Details", "FootballFields", new { id = fieldId });
            }

            // Create review
            var review = new Review
            {
                UserId = userId!,
                FootballFieldId = fieldId,
                Rating = rating,
                Comment = comment.Trim(),
                BookingId = bookingId,
                CreatedDate = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update field rating
            await UpdateFieldRating(fieldId);

            _logger.LogInformation($"User {userId} reviewed field {fieldId}");
            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";

            return RedirectToAction("Details", "FootballFields", new { id = fieldId });
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews
                .Include(r => r.FootballField)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
            {
                TempData["Error"] = "Không tìm thấy đánh giá!";
                return RedirectToAction("Index", "Home");
            }

            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
            {
                TempData["Error"] = "Không tìm thấy đánh giá!";
                return RedirectToAction("Index", "Home");
            }

            // Validation
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment) || comment.Length < 10)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Edit", new { id });
            }

            review.Rating = rating;
            review.Comment = comment.Trim();
            review.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            await UpdateFieldRating(review.FootballFieldId);

            TempData["Success"] = "Đã cập nhật đánh giá!";
            return RedirectToAction("Details", "FootballFields", new { id = review.FootballFieldId });
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
            {
                TempData["Error"] = "Không tìm thấy đánh giá!";
                return RedirectToAction("Index", "Home");
            }

            var fieldId = review.FootballFieldId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            await UpdateFieldRating(fieldId);

            TempData["Success"] = "Đã xóa đánh giá!";
            return RedirectToAction("Details", "FootballFields", new { id = fieldId });
        }

        // Helper: Update field average rating
        private async Task UpdateFieldRating(int fieldId)
        {
            var field = await _context.FootballFields.FindAsync(fieldId);
            if (field == null) return;

            var reviews = await _context.Reviews
                .Where(r => r.FootballFieldId == fieldId)
                .ToListAsync();

            field.TotalReviews = reviews.Count;
            field.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            await _context.SaveChangesAsync();
        }
    }
}