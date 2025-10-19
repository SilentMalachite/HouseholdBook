namespace HouseholdBook.Models.ViewModels
{
    public class HouseholdInsight
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount { get; set; }
        public IEnumerable<string> Advice { get; set; } = Enumerable.Empty<string>();
    }

    public class ComparativeReport
    {
        public MonthlyReportViewModel Current { get; set; } = null!;
        public MonthlyReportViewModel PreviousMonth { get; set; } = null!;
        public MonthlyReportViewModel PreviousYear { get; set; } = null!;
    }
}
