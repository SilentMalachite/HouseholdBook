using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "Cash";

        [Range(0, double.MaxValue)]
        public decimal InitialBalance { get; set; }

        public decimal CurrentBalance { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
