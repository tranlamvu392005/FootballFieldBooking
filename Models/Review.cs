using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FootballFieldBooking_New.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5 sao")]
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Đánh giá phải từ 10 đến 1000 ký tự")]
        [Display(Name = "Nội dung đánh giá")]
        public string Comment { get; set; } = string.Empty;

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int FootballFieldId { get; set; }

        public int? BookingId { get; set; } // Optional: Link to booking

        // Navigation Properties
        public IdentityUser User { get; set; } = null!;
        public FootballField FootballField { get; set; } = null!;
        public Booking? Booking { get; set; }
    }
}