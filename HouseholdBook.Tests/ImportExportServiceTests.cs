using System.Text;
using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HouseholdBook.Tests;

public class ImportExportServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ExportTransactionsToCsvAsync_ReturnsCsvContent()
    {
        await using var context = CreateContext();
        var category = new Category { Name = "食費", Type = "Expense" };
        var account = new Account { Name = "現金", Type = "Cash" };
        context.Categories.Add(category);
        context.Accounts.Add(account);
        context.Transactions.Add(new Transaction
        {
            Amount = -1200,
            Date = new DateTime(2024, 10, 1),
            Category = category,
            Account = account,
            Description = "ランチ"
        });
        await context.SaveChangesAsync();

        var service = new ImportExportService(context);
        var bytes = await service.ExportTransactionsToCsvAsync();
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.Contains("ランチ", csv);
        Assert.Contains("食費", csv);
    }
}
