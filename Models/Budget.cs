using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class Budget
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
