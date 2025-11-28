using System.ComponentModel.DataAnnotations;

namespace FootballFieldBooking_New.Models
{
    public class FieldType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // "Sân 5 người", "Sân 7 người", "Sân 11 người"

        [StringLength(200)]
        public string? Description { get; set; }

        public int PlayerCount { get; set; } // 5, 7, 11

        // Navigation property
        public ICollection<FootballField> FootballFields { get; set; } = new List<FootballField>();
    }
}