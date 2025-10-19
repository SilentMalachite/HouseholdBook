using Microsoft.EntityFrameworkCore;
using HouseholdBook.Models;

namespace HouseholdBook.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; } = null!;
        public DbSet<AccountBalanceSnapshot> AccountBalanceSnapshots { get; set; } = null!;
        public DbSet<SavingsGoal> SavingsGoals { get; set; } = null!;
        public DbSet<SavingsContribution> SavingsContributions { get; set; } = null!;
        public DbSet<Alert> Alerts { get; set; } = null!;
        public DbSet<AutoCategorizationRule> AutoCategorizationRules { get; set; } = null!;
    }
}