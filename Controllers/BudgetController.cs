using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HouseholdBook.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ApplicationDbContext _context;

        public BudgetController(IBudgetService budgetService, ApplicationDbContext context)
        {
            _budgetService = budgetService;
            _context = context;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            var budgets = await _budgetService.GetBudgetsAsync(targetYear, targetMonth);
            ViewData["Year"] = targetYear;
            ViewData["Month"] = targetMonth;
            return View(budgets);
        }

        public IActionResult Create(int? year, int? month)
        {
            PrepareSelectLists();
            var model = new Budget
            {
                Year = year ?? DateTime.Now.Year,
                Month = month ?? DateTime.Now.Month
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Budget budget)
        {
            if (ModelState.IsValid)
            {
                await _budgetService.CreateAsync(budget);
                return RedirectToAction(nameof(Index), new { year = budget.Year, month = budget.Month });
            }
            PrepareSelectLists();
            return View(budget);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _budgetService.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            PrepareSelectLists();
            return View(budget);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Budget budget)
        {
            if (id != budget.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PrepareSelectLists();
                return View(budget);
            }

            await _budgetService.UpdateAsync(budget);
            return RedirectToAction(nameof(Index), new { year = budget.Year, month = budget.Month });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var budget = await _budgetService.FindAsync(id);
            if (budget == null)
            {
                return NotFound();
            }
            return View(budget);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _budgetService.FindAsync(id);
            if (budget != null)
            {
                await _budgetService.DeleteAsync(id);
                return RedirectToAction(nameof(Index), new { year = budget.Year, month = budget.Month });
            }
            return RedirectToAction(nameof(Index));
        }

        private void PrepareSelectLists()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
        }
    }
}
