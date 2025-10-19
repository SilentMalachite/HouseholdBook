using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HouseholdBook.Models;
using HouseholdBook.Services;
using HouseholdBook.Models.ViewModels;

namespace HouseholdBook.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IReportService _reportService;

    public HomeController(ILogger<HomeController> logger, IReportService reportService)
    {
        _logger = logger;
        _reportService = reportService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> MonthlySummary(int? year, int? month)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var targetMonth = month ?? DateTime.Now.Month;
        var report = await _reportService.GetMonthlyReportAsync(targetYear, targetMonth);
        return View(report);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
