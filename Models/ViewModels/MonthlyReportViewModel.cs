using HouseholdBook.Models;

namespace HouseholdBook.Models.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public IEnumerable<MonthlyCategorySummary> CategorySummaries { get; set; } = Enumerable.Empty<MonthlyCategorySummary>();
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount => TotalIncome + TotalExpense;
    }

    public class MonthlyCategorySummary
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Difference => BudgetAmount - ActualAmount;
        public bool IsOverBudget => ActualAmount > BudgetAmount && BudgetAmount > 0;
    }
}
