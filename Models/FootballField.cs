using System.ComponentModel.DataAnnotations;

namespace FootballFieldBooking_New.Models
{
    public class FootballField
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Sân A1", "Sân B2"

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty; // "Khu A, Tầng 1"

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Foreign Key
        public int FieldTypeId { get; set; }

        // Navigation properties
        public FieldType FieldType { get; set; } = null!;
        public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        [Display(Name = "Đánh giá trung bình")]
        public double AverageRating { get; set; } = 0;

        [Display(Name = "Số lượng đánh giá")]
        public int TotalReviews { get; set; } = 0;
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}