using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IBudgetService
    {
        Task<IEnumerable<Budget>> GetBudgetsAsync(int year, int month);
        Task<Budget?> FindAsync(int id);
        Task CreateAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int id);
    }

    public class BudgetService(ApplicationDbContext context) : IBudgetService
    {
        public async Task<IEnumerable<Budget>> GetBudgetsAsync(int year, int month)
        {
            return await context.Budgets
                .Include(b => b.Category)
                .Where(b => b.Year == year && b.Month == month)
                .OrderBy(b => b.Category.Name)
                .ToListAsync();
        }

        public async Task<Budget?> FindAsync(int id)
        {
            return await context.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task CreateAsync(Budget budget)
        {
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Budget budget)
        {
            context.Budgets.Update(budget);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var budget = await context.Budgets.FindAsync(id);
            if (budget != null)
            {
                context.Budgets.Remove(budget);
                await context.SaveChangesAsync();
            }
        }
    }
}
