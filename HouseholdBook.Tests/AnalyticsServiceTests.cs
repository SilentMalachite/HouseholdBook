using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class AnalyticsServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GenerateAlertsAsync_FlagsBudgetOverrun()
    {
        await using var context = CreateContext();
        var category = new Category { Name = "外食", Type = "Expense" };
        context.Categories.Add(category);
        context.Budgets.Add(new Budget { Category = category, Year = 2024, Month = 10, Amount = 20000 });
        context.Transactions.Add(new Transaction
        {
            Category = category,
            Amount = -25000,
            Date = new DateTime(2024, 10, 10)
        });
        await context.SaveChangesAsync();

        var reportService = new ReportService(context);
        var analytics = new AnalyticsService(context, reportService);

        var alerts = await analytics.GenerateAlertsAsync(2024, 10);

        Assert.Single(alerts);
        Assert.Contains("超過", alerts.First().Message);
    }
}
