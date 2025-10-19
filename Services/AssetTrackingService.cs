using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Services
{
    public interface IAssetTrackingService
    {
        Task RecordSnapshotAsync(int accountId, decimal balance, string? notes = null);
        Task<IEnumerable<AccountBalanceSnapshot>> GetSnapshotsAsync(int accountId, DateTime? from = null, DateTime? to = null);
        Task<decimal> GetCurrentBalanceAsync(int accountId);
        Task ReconcileCashAsync(int accountId, decimal actualBalance, string? notes = null);
    }

    public class AssetTrackingService(ApplicationDbContext context) : IAssetTrackingService
    {
        public async Task RecordSnapshotAsync(int accountId, decimal balance, string? notes = null)
        {
            context.AccountBalanceSnapshots.Add(new AccountBalanceSnapshot
            {
                AccountId = accountId,
                Balance = balance,
                SnapshotDate = DateTime.UtcNow,
                Notes = notes
            });

            var account = await context.Accounts.FindAsync(accountId);
            if (account != null)
            {
                account.CurrentBalance = balance;
            }

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<AccountBalanceSnapshot>> GetSnapshotsAsync(int accountId, DateTime? from = null, DateTime? to = null)
        {
            var query = context.AccountBalanceSnapshots
                .Where(s => s.AccountId == accountId)
                .OrderByDescending(s => s.SnapshotDate)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(s => s.SnapshotDate >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(s => s.SnapshotDate <= to.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<decimal> GetCurrentBalanceAsync(int accountId)
        {
            var account = await context.Accounts.FindAsync(accountId);
            return account?.CurrentBalance ?? 0m;
        }

        public async Task ReconcileCashAsync(int accountId, decimal actualBalance, string? notes = null)
        {
            var snapshots = await GetSnapshotsAsync(accountId, DateTime.UtcNow.Date);
            if (!snapshots.Any())
            {
                await RecordSnapshotAsync(accountId, actualBalance, notes);
            }
            else
            {
                var latest = snapshots.First();
                if (latest.Balance != actualBalance)
                {
                    await RecordSnapshotAsync(accountId, actualBalance, notes ?? "差異調整");
                }
            }
        }
    }
}
