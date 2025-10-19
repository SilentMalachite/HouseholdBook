using HouseholdBook.Services;
using Microsoft.AspNetCore.Mvc;

namespace HouseholdBook.Controllers
{
    public class AnalyticsController(IAnalyticsService analyticsService) : Controller
    {
        public async Task<IActionResult> Dashboard(int? year, int? month)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var targetMonth = month ?? DateTime.Now.Month;
            var insight = await analyticsService.GetHouseholdInsightAsync(targetYear, targetMonth);
            var comparative = await analyticsService.GetComparativeReportAsync(targetYear, targetMonth);
            var alerts = await analyticsService.GenerateAlertsAsync(targetYear, targetMonth);

            ViewData["Alerts"] = alerts;
            ViewData["Comparative"] = comparative;
            return View(insight);
        }
    }
}
