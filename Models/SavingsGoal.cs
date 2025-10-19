using System.ComponentModel.DataAnnotations;

namespace HouseholdBook.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CurrentAmount { get; set; }

        public DateTime? TargetDate { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public ICollection<SavingsContribution> Contributions { get; set; } = new List<SavingsContribution>();
    }

    public class SavingsContribution
    {
        public int Id { get; set; }

        [Required]
        public int SavingsGoalId { get; set; }
        public SavingsGoal SavingsGoal { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ContributionDate { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
