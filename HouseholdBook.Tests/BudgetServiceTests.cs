using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class BudgetServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetBudgetsAsync_FiltersByYearAndMonth()
    {
        await using var context = CreateContext();
        var category = new Category { Name = "娯楽", Type = "Expense" };
        context.Categories.Add(category);
        context.Budgets.AddRange(
            new Budget { Category = category, Year = 2024, Month = 10, Amount = 10000 },
            new Budget { Category = category, Year = 2024, Month = 9, Amount = 5000 });
        await context.SaveChangesAsync();

        var service = new BudgetService(context);

        var budgets = await service.GetBudgetsAsync(2024, 10);

        var single = Assert.Single(budgets);
        Assert.Equal(10000, single.Amount);
        Assert.Equal(10, single.Month);
    }
}
