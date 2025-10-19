namespace HouseholdBook.Models.ViewModels
{
    public class SavingsGoalSummary
    {
        public SavingsGoal Goal { get; set; } = null!;
        public decimal ProgressPercentage => Goal.TargetAmount == 0 ? 0 : (Goal.CurrentAmount / Goal.TargetAmount) * 100;
        public decimal RemainingAmount => Goal.TargetAmount - Goal.CurrentAmount;
    }
}
