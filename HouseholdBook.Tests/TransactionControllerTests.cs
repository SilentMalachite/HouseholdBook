using HouseholdBook.Controllers;
using HouseholdBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class TransactionControllerTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Index_ReturnsTransactionsWithCategories()
    {
        await using var context = CreateContext();
        var account = new Account { Name = "現金", Type = "Cash", InitialBalance = 0, CurrentBalance = 0 };
        var category = new Category { Name = "交通", Type = "Expense" };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        context.Transactions.Add(new Transaction
        {
            Amount = 1200,
            Date = new DateTime(2024, 10, 1),
            Account = account,
            Category = category,
            Description = "バス代"
        });
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);

        var result = await controller.Index() as ViewResult;

        Assert.NotNull(result?.ViewData.Model);
        var model = Assert.IsAssignableFrom<IEnumerable<Transaction>>(result!.ViewData.Model);
        var transaction = Assert.Single(model);
        Assert.Equal("交通", transaction.Category.Name);
    }

    [Fact]
    public async Task Create_ValidTransactionRedirectsToIndex()
    {
        await using var context = CreateContext();
        var account = new Account { Name = "銀行", Type = "Bank", InitialBalance = 0, CurrentBalance = 0 };
        var category = new Category { Name = "給料", Type = "Income" };
        context.Accounts.Add(account);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = new TransactionController(context);
        var transaction = new Transaction
        {
            Amount = 50000,
            Date = new DateTime(2024, 10, 2),
            AccountId = account.Id,
            CategoryId = category.Id,
            Description = "10月の給料"
        };

        var result = await controller.Create(transaction);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(TransactionController.Index), redirect.ActionName);

        var saved = await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .SingleAsync();
        Assert.Equal(50000, saved.Amount);
        Assert.Equal("給料", saved.Category.Name);
        Assert.Equal("銀行", saved.Account.Name);
    }
}
