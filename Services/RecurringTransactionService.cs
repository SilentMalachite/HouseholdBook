using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IRecurringTransactionService
    {
        Task ProcessDueTransactionsAsync(DateTime runDate);
        Task<IEnumerable<RecurringTransaction>> GetAllAsync();
        Task<RecurringTransaction?> FindAsync(int id);
        Task CreateAsync(RecurringTransaction transaction);
        Task UpdateAsync(RecurringTransaction transaction);
        Task DeleteAsync(int id);
    }

    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ApplicationDbContext _context;

        public RecurringTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ProcessDueTransactionsAsync(DateTime runDate)
        {
            var dueTransactions = await _context.RecurringTransactions
                .Where(r => r.NextRunDate.Date <= runDate.Date && (r.EndDate == null || r.EndDate.Value.Date >= runDate.Date))
                .ToListAsync();

            foreach (var recurring in dueTransactions)
            {
                var transaction = new Transaction
                {
                    Amount = recurring.Amount,
                    Date = runDate,
                    AccountId = recurring.AccountId,
                    CategoryId = recurring.CategoryId,
                    Description = recurring.Description
                };

                _context.Transactions.Add(transaction);

                recurring.NextRunDate = recurring.Frequency switch
                {
                    Frequency.Daily => recurring.NextRunDate.AddDays(1),
                    Frequency.Weekly => recurring.NextRunDate.AddDays(7),
                    Frequency.Monthly => recurring.NextRunDate.AddMonths(1),
                    Frequency.Yearly => recurring.NextRunDate.AddYears(1),
                    _ => recurring.NextRunDate
                };
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecurringTransaction>> GetAllAsync()
        {
            return await _context.RecurringTransactions
                .Include(r => r.Category)
                .Include(r => r.Account)
                .OrderBy(r => r.NextRunDate)
                .ToListAsync();
        }

        public async Task<RecurringTransaction?> FindAsync(int id)
        {
            return await _context.RecurringTransactions
                .Include(r => r.Category)
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CreateAsync(RecurringTransaction transaction)
        {
            _context.RecurringTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecurringTransaction transaction)
        {
            _context.RecurringTransactions.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var recurring = await _context.RecurringTransactions.FindAsync(id);
            if (recurring != null)
            {
                _context.RecurringTransactions.Remove(recurring);
                await _context.SaveChangesAsync();
            }
        }
    }
}
