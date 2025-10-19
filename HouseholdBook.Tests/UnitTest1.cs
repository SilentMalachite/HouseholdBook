using HouseholdBook.Models;
using Xunit;

namespace HouseholdBook.Tests;

public class CategoryTests
{
    [Theory]
    [InlineData("食費")]
    [InlineData("給料")]
    public void Category_DefaultTypeIsExpense(string name)
    {
        var category = new Category { Name = name };

        Assert.Equal("Expense", category.Type);
    }
}
