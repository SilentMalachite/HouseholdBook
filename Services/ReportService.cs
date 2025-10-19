using HouseholdBook.Models;
using HouseholdBook.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IReportService
    {
        Task<MonthlyReportViewModel> GetMonthlyReportAsync(int year, int month);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyReportViewModel> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.Year == year && b.Month == month)
                .ToListAsync();

            var summaries = transactions
                .GroupBy(t => t.Category.Name)
                .Select(group => new MonthlyCategorySummary
                {
                    CategoryName = group.Key,
                    ActualAmount = group.Sum(t => t.Amount),
                    BudgetAmount = budgets.FirstOrDefault(b => b.Category.Name == group.Key)?.Amount ?? 0
                })
                .ToList();

            foreach (var budget in budgets)
            {
                if (!summaries.Any(s => s.CategoryName == budget.Category.Name))
                {
                    summaries.Add(new MonthlyCategorySummary
                    {
                        CategoryName = budget.Category.Name,
                        BudgetAmount = budget.Amount,
                        ActualAmount = 0
                    });
                }
            }

            var totalIncome = transactions.Where(t => t.Amount >= 0).Sum(t => t.Amount);
            var totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => t.Amount);

            return new MonthlyReportViewModel
            {
                Year = year,
                Month = month,
                CategorySummaries = summaries.OrderBy(s => s.CategoryName).ToList(),
                TotalIncome = totalIncome,
                TotalExpense = totalExpense
            };
        }
    }
}

