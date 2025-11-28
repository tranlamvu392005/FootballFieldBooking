using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballFieldBooking_New.Models
{
    public class TimeSlot
    {
        public int Id { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; } // 06:00

        [Required]
        public TimeSpan EndTime { get; set; } // 08:00

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerHour { get; set; } // 200000

        public bool IsWeekend { get; set; } = false; // Giá cuối tuần khác

        // Foreign Key
        public int FootballFieldId { get; set; }

        // Navigation properties
        public FootballField FootballField { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}