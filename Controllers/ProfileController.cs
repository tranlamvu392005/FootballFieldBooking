using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FootballFieldBooking_New.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            var bookings = await _context.Bookings
                .Include(b => b.FootballField)
                .Include(b => b.TimeSlot)
                .Include(b => b.Payment)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                User = user,
                Profile = profile,
                RecentBookings = bookings,
                TotalBookings = await _context.Bookings.CountAsync(b => b.UserId == userId),
                TotalSpent = await _context.Bookings
                    .Where(b => b.UserId == userId && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice)
            };

            return View(viewModel);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            var viewModel = new EditProfileViewModel
            {
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                FullName = profile?.FullName ?? "",
                Address = profile?.Address ?? "",
                DateOfBirth = profile?.DateOfBirth,
                Gender = profile?.Gender ?? ""
            };

            return View(viewModel);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId!);

                if (user == null)
                    return NotFound();

                // Update user info
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);

                // Update or create profile
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null)
                {
                    profile = new UserProfile { UserId = userId! };
                    _context.UserProfiles.Add(profile);
                }

                profile.FullName = model.FullName;
                profile.Address = model.Address;
                profile.DateOfBirth = model.DateOfBirth;
                profile.Gender = model.Gender;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }

    // View Models
    public class UserProfileViewModel
    {
        public IdentityUser User { get; set; } = null!;
        public UserProfile? Profile { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class EditProfileViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = "";

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = "";
    }
}