using System.ComponentModel.DataAnnotations;
using FootballFieldBooking_New.Data;
using FootballFieldBooking_New.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballFieldBooking_New.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string searchTerm = "", string role = "")
        {
            var users = _userManager.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.Email.Contains(searchTerm) || u.UserName.Contains(searchTerm));
            }

            var userList = await users.ToListAsync();

            // Get roles for each user
            var userViewModels = new List<UserViewModel>();
            foreach (var user in userList)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (string.IsNullOrEmpty(role) || roles.Contains(role))
                {
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        UserName = user.UserName ?? "",
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber ?? "",
                        LockoutEnd = user.LockoutEnd,
                        Roles = string.Join(", ", roles),
                        FullName = profile?.FullName ?? "N/A",
                        CreatedDate = profile?.CreatedDate ?? DateTime.Now
                    });
                }
            }

            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentRole"] = role;

            return View(userViewModels);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == id);
            var bookings = await _context.Bookings
                .Include(b => b.FootballField)
                .Include(b => b.TimeSlot)
                .Where(b => b.UserId == id)
                .OrderByDescending(b => b.BookingDate)
                .Take(10)
                .ToListAsync();

            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList(),
                Profile = profile,
                RecentBookings = bookings,
                TotalBookings = await _context.Bookings.CountAsync(b => b.UserId == id),
                TotalSpent = await _context.Bookings
                    .Where(b => b.UserId == id && b.Status == BookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice)
            };

            return View(viewModel);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == id);

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                IsAdmin = roles.Contains("Admin"),
                FullName = profile?.FullName ?? "",
                Address = profile?.Address ?? "",
                DateOfBirth = profile?.DateOfBirth,
                Gender = profile?.Gender ?? ""
            };

            return View(viewModel);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                // Update user info
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (model.IsAdmin && !currentRoles.Contains("Admin"))
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else if (!model.IsAdmin && currentRoles.Contains("Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                }

                // Update or create profile
                var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == id);
                if (profile == null)
                {
                    profile = new UserProfile { UserId = id };
                    _context.UserProfiles.Add(profile);
                }

                profile.FullName = model.FullName;
                profile.Address = model.Address;
                profile.DateOfBirth = model.DateOfBirth;
                profile.Gender = model.Gender;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật người dùng thành công!";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(model);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Check if user has bookings
            var hasBookings = await _context.Bookings.AnyAsync(b => b.UserId == id);
            if (hasBookings)
            {
                TempData["Error"] = "Không thể xóa người dùng có lịch sử đặt sân!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Xóa người dùng thành công!";
            }
            else
            {
                TempData["Error"] = "Có lỗi khi xóa người dùng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/ToggleLock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now)
            {
                // Unlock
                user.LockoutEnd = null;
                TempData["Success"] = "Đã mở khóa tài khoản!";
            }
            else
            {
                // Lock for 100 years
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
                TempData["Success"] = "Đã khóa tài khoản!";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // View Models
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string Roles { get; set; } = "";
        public string FullName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
    }

    public class UserDetailViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public List<string> Roles { get; set; } = new();
        public UserProfile? Profile { get; set; }
        public List<Booking> RecentBookings { get; set; } = new();
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Phone]
        public string PhoneNumber { get; set; } = "";

        public bool IsAdmin { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        public string Address { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }

        public string Gender { get; set; } = "";
    }
}