using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HouseholdBook.Models;
using HouseholdBook.Services;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HouseholdBook.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAutoCategorizationService _autoCategorizationService;
        private readonly IImportExportService _importExportService;

        public TransactionController(ApplicationDbContext context, IAutoCategorizationService autoCategorizationService, IImportExportService importExportService)
        {
            _context = context;
            _autoCategorizationService = autoCategorizationService;
            _importExportService = importExportService;
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var transactions = _context.Transactions.Include(t => t.Category).Include(t => t.Account);
            return View(await transactions.ToListAsync());
        }

        public async Task<FileResult> ExportCsv(DateTime? from, DateTime? to)
        {
            var bytes = await _importExportService.ExportTransactionsToCsvAsync(from, to);
            return File(bytes, "text/csv", $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        public async Task<FileResult> ExportWorksheet(DateTime? from, DateTime? to)
        {
            var bytes = await _importExportService.ExportTransactionsToWorksheetAsync(from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"transactions_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "CSVファイルを選択してください。");
                return RedirectToAction(nameof(Index));
            }

            await using var stream = file.OpenReadStream();
            await _importExportService.ImportTransactionsFromCsvAsync(stream);
            return RedirectToAction(nameof(Index));
        }

        // GET: Transaction/Calendar
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var currentMonth = month ?? DateTime.Now.Month;

            ViewData["CurrentYear"] = currentYear;
            ViewData["CurrentMonth"] = currentMonth;

            var startDate = new DateTime(currentYear, currentMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderBy(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }

        // GET: Transaction/CreateForDate
        public IActionResult CreateForDate(string date)
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            var transaction = new Transaction();
            if (DateTime.TryParse(date, out var parsedDate))
            {
                transaction.Date = parsedDate;
            }
            return View("Create", transaction);
        }

        // GET: Transaction/GetTransactionsForDate
        public async Task<IActionResult> GetTransactionsForDate(string date)
        {
            if (DateTime.TryParse(date, out var parsedDate))
            {
                var transactions = await _context.Transactions
                    .Include(t => t.Category)
                    .Include(t => t.Account)
                    .Where(t => t.Date.Date == parsedDate.Date)
                    .OrderBy(t => t.Date)
                    .ToListAsync();

                return Json(transactions.Select(t => new
                {
                    t.Id,
                    t.Amount,
                    Date = t.Date.ToString("yyyy-MM-dd"),
                    CategoryName = t.Category.Name,
                    AccountName = t.Account.Name,
                    t.Description
                }));
            }
            return Json(new List<object>());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name");
            return View();
        }

        // POST: Transaction/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,Date,CategoryId,AccountId,Description")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                await _autoCategorizationService.ApplySuggestionAsync(transaction);
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Amount,Date,CategoryId,AccountId,Description")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _autoCategorizationService.ApplySuggestionAsync(transaction);
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", transaction.CategoryId);
            ViewData["AccountId"] = new SelectList(_context.Accounts, "Id", "Name", transaction.AccountId);
            return View(transaction);
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}