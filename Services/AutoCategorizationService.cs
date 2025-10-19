using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IAutoCategorizationService
    {
        Task<Category?> SuggestCategoryAsync(Transaction transaction);
        Task ApplySuggestionAsync(Transaction transaction);
        Task<IEnumerable<AutoCategorizationRule>> GetRulesAsync();
        Task CreateRuleAsync(AutoCategorizationRule rule);
        Task DeleteRuleAsync(int id);
    }

    public class AutoCategorizationService(ApplicationDbContext context) : IAutoCategorizationService
    {
        public async Task<Category?> SuggestCategoryAsync(Transaction transaction)
        {
            // Rule-based matching by keyword
            var description = transaction.Description?.ToLowerInvariant() ?? string.Empty;
            var rules = await context.AutoCategorizationRules
                .Include(r => r.Category)
                .OrderByDescending(r => r.Priority ?? 0)
                .ThenByDescending(r => r.MatchCount)
                .ToListAsync();

            foreach (var rule in rules)
            {
                if (!string.IsNullOrWhiteSpace(rule.Keyword) && description.Contains(rule.Keyword.ToLowerInvariant()))
                {
                    rule.MatchCount++;
                    await context.SaveChangesAsync();
                    return rule.Category;
                }
            }

            // Heuristic based on past transactions
            if (!string.IsNullOrWhiteSpace(description))
            {
                var historyCategory = await context.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.CategoryId != 0 && t.Description != null && description.Contains(t.Description.ToLower()))
                    .OrderByDescending(t => t.Date)
                    .Select(t => t.Category)
                    .FirstOrDefaultAsync();

                if (historyCategory != null)
                {
                    return historyCategory;
                }
            }

            return null;
        }

        public async Task ApplySuggestionAsync(Transaction transaction)
        {
            if (transaction.CategoryId != 0)
            {
                return;
            }

            var suggested = await SuggestCategoryAsync(transaction);
            if (suggested != null)
            {
                transaction.CategoryId = suggested.Id;
            }
        }

        public async Task<IEnumerable<AutoCategorizationRule>> GetRulesAsync()
        {
            return await context.AutoCategorizationRules.Include(r => r.Category).OrderByDescending(r => r.Priority ?? 0).ToListAsync();
        }

        public async Task CreateRuleAsync(AutoCategorizationRule rule)
        {
            context.AutoCategorizationRules.Add(rule);
            await context.SaveChangesAsync();
        }

        public async Task DeleteRuleAsync(int id)
        {
            var rule = await context.AutoCategorizationRules.FindAsync(id);
            if (rule != null)
            {
                context.AutoCategorizationRules.Remove(rule);
                await context.SaveChangesAsync();
            }
        }
    }
}
