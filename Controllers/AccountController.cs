using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBook.Controllers
{
    public class AccountController(ApplicationDbContext context, IAssetTrackingService assetTrackingService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var accounts = await context.Accounts.Include(a => a.Transactions).ToListAsync();
            return View(accounts);
        }

        public IActionResult Create()
        {
            return View(new Account { Type = "Cash" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Account account)
        {
            if (!ModelState.IsValid)
            {
                return View(account);
            }

            account.CurrentBalance = account.InitialBalance;
            context.Accounts.Add(account);
            await context.SaveChangesAsync();
            await assetTrackingService.RecordSnapshotAsync(account.Id, account.CurrentBalance, "初期登録");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var account = await context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(account);
            }

            var existing = await context.Accounts.FindAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Name = account.Name;
            existing.Type = account.Type;
            existing.Notes = account.Notes;
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> BalanceHistory(int id)
        {
            var account = await context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            ViewData["Account"] = account;
            var snapshots = await assetTrackingService.GetSnapshotsAsync(id, DateTime.UtcNow.AddMonths(-6));
            return View(snapshots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reconcile(int id, decimal actualBalance, string? notes)
        {
            await assetTrackingService.ReconcileCashAsync(id, actualBalance, notes);
            return RedirectToAction(nameof(BalanceHistory), new { id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var account = await context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await context.Accounts.FindAsync(id);
            if (account != null)
            {
                context.Accounts.Remove(account);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
