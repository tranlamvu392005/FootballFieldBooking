using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FootballFieldBooking_New.Models
{
    public enum PaymentMethod
    {
        Cash,           // Tiền mặt
        BankTransfer,   // Chuyển khoản
        EWallet         // Ví điện tử
    }

    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        public bool IsPaid { get; set; } = false;

        // Foreign Key
        [Required]
        public int BookingId { get; set; }

        // Navigation property
        public Booking Booking { get; set; } = null!;
    }
}