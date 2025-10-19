using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HouseholdBook.Controllers
{
    public class RecurringTransactionController : Controller
    {
        private readonly IRecurringTransactionService _service;
        private readonly ApplicationDbContext _context;

        public RecurringTransactionController(IRecurringTransactionService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _service.GetAllAsync();
            return View(items);
        }

        public IActionResult Create()
        {
            PrepareSelectLists();
            var model = new RecurringTransaction
            {
                StartDate = DateTime.Today,
                NextRunDate = DateTime.Today,
                Frequency = Frequency.Monthly
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecurringTransaction recurringTransaction)
        {
            if (!ModelState.IsValid)
            {
                PrepareSelectLists();
                return View(recurringTransaction);
            }

            await _service.CreateAsync(recurringTransaction);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var recurring = await _service.FindAsync(id);
            if (recurring == null)
            {
                return NotFound();
            }
            PrepareSelectLists();
            return View(recurring);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecurringTransaction recurringTransaction)
        {
            if (id != recurringTransaction.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PrepareSelectLists();
                return View(recurringTransaction);
            }

            await _service.UpdateAsync(recurringTransaction);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var recurring = await _service.FindAsync(id);
            if (recurring == null)
            {
                return NotFound();
            }
            return View(recurring);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void PrepareSelectLists()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
            ViewData["AccountId"] = new SelectList(_context.Accounts.OrderBy(a => a.Name), "Id", "Name");
        }
    }
}
