using HouseholdBook.Models;
using HouseholdBook.Models.ViewModels;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class ReportServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetMonthlyReportAsync_ReturnsBudgetSummaries()
    {
        await using var context = CreateContext();
        var category = new Category { Name = "食費", Type = "Expense" };
        context.Categories.Add(category);
        context.Budgets.Add(new Budget
        {
            Category = category,
            Year = 2024,
            Month = 10,
            Amount = 30000
        });
        context.Transactions.Add(new Transaction
        {
            Category = category,
            Amount = -15000,
            Date = new DateTime(2024, 10, 5)
        });
        await context.SaveChangesAsync();

        var reportService = new ReportService(context);

        MonthlyReportViewModel report = await reportService.GetMonthlyReportAsync(2024, 10);

        Assert.Equal(2024, report.Year);
        Assert.Equal(10, report.Month);
        Assert.Single(report.CategorySummaries);
        var summary = report.CategorySummaries.First();
        Assert.Equal("食費", summary.CategoryName);
        Assert.Equal(30000, summary.BudgetAmount);
        Assert.Equal(-15000, summary.ActualAmount);
        Assert.False(summary.IsOverBudget);
    }
}
