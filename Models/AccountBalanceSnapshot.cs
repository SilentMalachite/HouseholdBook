using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class AccountBalanceSnapshot
    {
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }
        public Account Account { get; set; } = null!;

        [Required]
        public DateTime SnapshotDate { get; set; }

        [Range(double.MinValue, double.MaxValue)]
        public decimal Balance { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }
    }
}
