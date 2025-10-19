using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface ISavingsGoalService
    {
        Task<IEnumerable<SavingsGoal>> GetGoalsAsync();
        Task<SavingsGoal?> FindAsync(int id);
        Task CreateGoalAsync(SavingsGoal goal);
        Task UpdateGoalAsync(SavingsGoal goal);
        Task DeleteGoalAsync(int id);
        Task AddContributionAsync(int goalId, decimal amount, DateTime date, string? description = null);
    }

    public class SavingsGoalService(ApplicationDbContext context) : ISavingsGoalService
    {
        public async Task<IEnumerable<SavingsGoal>> GetGoalsAsync()
        {
            return await context.SavingsGoals
                .Include(g => g.Contributions)
                .OrderBy(g => g.TargetDate ?? DateTime.MaxValue)
                .ToListAsync();
        }

        public async Task<SavingsGoal?> FindAsync(int id)
        {
            return await context.SavingsGoals
                .Include(g => g.Contributions)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task CreateGoalAsync(SavingsGoal goal)
        {
            context.SavingsGoals.Add(goal);
            await context.SaveChangesAsync();
        }

        public async Task UpdateGoalAsync(SavingsGoal goal)
        {
            context.SavingsGoals.Update(goal);
            await context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(int id)
        {
            var goal = await context.SavingsGoals.FindAsync(id);
            if (goal != null)
            {
                context.SavingsGoals.Remove(goal);
                await context.SaveChangesAsync();
            }
        }

        public async Task AddContributionAsync(int goalId, decimal amount, DateTime date, string? description = null)
        {
            var goal = await context.SavingsGoals.FindAsync(goalId);
            if (goal == null)
            {
                return;
            }

            context.SavingsContributions.Add(new SavingsContribution
            {
                SavingsGoalId = goalId,
                Amount = amount,
                ContributionDate = date,
                Description = description
            });

            goal.CurrentAmount += amount;
            await context.SaveChangesAsync();
        }
    }
}
