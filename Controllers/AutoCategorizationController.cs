using HouseholdBook.Models;
using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HouseholdBook.Controllers
{
    public class AutoCategorizationController(IAutoCategorizationService service, ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var rules = await service.GetRulesAsync();
            return View(rules);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(context.Categories.OrderBy(c => c.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AutoCategorizationRule rule)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(context.Categories.OrderBy(c => c.Name), "Id", "Name", rule.CategoryId);
                return View(rule);
            }
            await service.CreateRuleAsync(rule);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteRuleAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
