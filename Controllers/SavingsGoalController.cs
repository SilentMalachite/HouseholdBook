using HouseholdBook.Models;
using HouseholdBook.Models.ViewModels;
using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdBook.Controllers
{
    public class SavingsGoalController(ISavingsGoalService service) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var goals = await service.GetGoalsAsync();
            var summaries = goals.Select(goal => new SavingsGoalSummary { Goal = goal }).ToList();
            return View(summaries);
        }

        public IActionResult Create()
        {
            return View(new SavingsGoal { TargetDate = DateTime.Today.AddMonths(6) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingsGoal goal)
        {
            if (!ModelState.IsValid)
            {
                return View(goal);
            }

            await service.CreateGoalAsync(goal);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var goal = await service.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SavingsGoal goal)
        {
            if (id != goal.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(goal);
            }

            await service.UpdateGoalAsync(goal);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var goal = await service.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }
            var summary = new SavingsGoalSummary { Goal = goal };
            return View(summary);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var goal = await service.FindAsync(id);
            if (goal == null)
            {
                return NotFound();
            }
            return View(goal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await service.DeleteGoalAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContribution(int id, decimal amount, DateTime contributionDate, string? description)
        {
            await service.AddContributionAsync(id, amount, contributionDate, description);
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
