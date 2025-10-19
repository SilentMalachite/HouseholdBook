using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}