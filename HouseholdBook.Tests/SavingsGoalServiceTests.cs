using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class SavingsGoalServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddContributionAsync_UpdatesCurrentAmount()
    {
        await using var context = CreateContext();
        var goal = new SavingsGoal
        {
            Name = "旅行資金",
            TargetAmount = 100000,
            CurrentAmount = 20000
        };
        context.SavingsGoals.Add(goal);
        await context.SaveChangesAsync();

        var service = new SavingsGoalService(context);
        await service.AddContributionAsync(goal.Id, 5000, new DateTime(2024, 10, 20), "臨時積立");

        var updated = await service.FindAsync(goal.Id);
        Assert.NotNull(updated);
        Assert.Equal(25000, updated!.CurrentAmount);
        var contribution = Assert.Single(updated.Contributions);
        Assert.Equal(5000, contribution.Amount);
    }
}
