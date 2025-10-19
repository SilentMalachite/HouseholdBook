using HouseholdBook.Models;
using HouseholdBook.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IAnalyticsService
    {
        Task<IEnumerable<Alert>> GenerateAlertsAsync(int year, int month);
        Task<HouseholdInsight> GetHouseholdInsightAsync(int year, int month);
        Task<ComparativeReport> GetComparativeReportAsync(int year, int month);
    }

    public class AnalyticsService(ApplicationDbContext context, IReportService reportService) : IAnalyticsService
    {
        public async Task<IEnumerable<Alert>> GenerateAlertsAsync(int year, int month)
        {
            var alerts = new List<Alert>();
            var report = await reportService.GetMonthlyReportAsync(year, month);

            foreach (var summary in report.CategorySummaries)
            {
                if (summary.IsOverBudget)
                {
                    alerts.Add(new Alert
                    {
                        Type = "Budget",
                        Message = $"{summary.CategoryName} の支出が予算を超過しています。差額: ¥{Math.Abs(summary.Difference):N0}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (Math.Abs(report.NetAmount) > Math.Abs(report.TotalIncome) * 0.5m)
            {
                alerts.Add(new Alert
                {
                    Type = "Balance",
                    Message = $"今月の収支差額 (¥{report.NetAmount:N0}) が収入の 50% を超えています。支出の見直しを検討してください。",
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (alerts.Any())
            {
                context.Alerts.AddRange(alerts);
                await context.SaveChangesAsync();
            }

            return alerts;
        }

        public async Task<HouseholdInsight> GetHouseholdInsightAsync(int year, int month)
        {
            var report = await reportService.GetMonthlyReportAsync(year, month);
            var topExpenses = report.CategorySummaries
                .Where(c => c.ActualAmount < 0)
                .OrderBy(c => c.ActualAmount)
                .Take(3)
                .Select(c => c.CategoryName)
                .ToList();

            var advice = new List<string>();
            if (topExpenses.Any())
            {
                advice.Add($"支出の大きいカテゴリ: {string.Join(", ", topExpenses)} を見直しましょう。");
            }

            if (report.NetAmount < 0)
            {
                advice.Add("今月は赤字です。予算配分を見直し、節約可能な支出を削減してください。");
            }
            else
            {
                advice.Add("黒字を維持しています。余剰分を貯蓄や投資に回すことを検討しましょう。");
            }

            return new HouseholdInsight
            {
                Year = year,
                Month = month,
                Advice = advice,
                NetAmount = report.NetAmount,
                TotalIncome = report.TotalIncome,
                TotalExpense = report.TotalExpense
            };
        }

        public async Task<ComparativeReport> GetComparativeReportAsync(int year, int month)
        {
            var current = await reportService.GetMonthlyReportAsync(year, month);
            var previousMonth = month == 1 ? 12 : month - 1;
            var previousYear = month == 1 ? year - 1 : year;
            var prevReport = await reportService.GetMonthlyReportAsync(previousYear, previousMonth);

            var previousYearReport = await reportService.GetMonthlyReportAsync(year - 1, month);

            return new ComparativeReport
            {
                Current = current,
                PreviousMonth = prevReport,
                PreviousYear = previousYearReport
            };
        }
    }
}
