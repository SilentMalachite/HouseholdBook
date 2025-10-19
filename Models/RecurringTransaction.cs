using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class RecurringTransaction
    {
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public Frequency Frequency { get; set; }

        public DateTime NextRunDate { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }

    public enum Frequency
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }
}
