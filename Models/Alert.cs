using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class Alert
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [StringLength(200)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }
    }
}
