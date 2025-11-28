using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FootballFieldBooking_New.Models
{
    public enum BookingStatus
    {
        Pending,      // Chờ xác nhận
        Confirmed,    // Đã xác nhận
        Cancelled,    // Đã hủy
        Completed     // Đã hoàn thành
    }

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now; // Ngày đặt

        [Required]
        public DateTime PlayDate { get; set; } // Ngày chơi

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int FootballFieldId { get; set; }

        [Required]
        public int TimeSlotId { get; set; }

        // Navigation properties
        public IdentityUser User { get; set; } = null!;
        public FootballField FootballField { get; set; } = null!;
        public TimeSlot TimeSlot { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}