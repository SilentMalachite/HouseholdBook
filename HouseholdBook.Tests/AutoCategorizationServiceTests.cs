using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class AutoCategorizationServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SuggestCategoryAsync_ReturnsMatchingRule()
    {
        await using var context = CreateContext();
        var category = new Category { Name = "食費", Type = "Expense" };
        context.Categories.Add(category);
        context.AutoCategorizationRules.Add(new AutoCategorizationRule
        {
            Keyword = "ランチ",
            Category = category,
            Priority = 10
        });
        await context.SaveChangesAsync();

        var service = new AutoCategorizationService(context);
        var transaction = new Transaction { Description = "会社のランチ代" };

        var suggested = await service.SuggestCategoryAsync(transaction);

        Assert.NotNull(suggested);
        Assert.Equal("食費", suggested!.Name);
    }
}
