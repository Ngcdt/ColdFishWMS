using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColdFishWMS.Business.Services;

namespace ColdFishWMS.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(int days = 14)
    {
        ViewBag.Days = days;
        var dashboard = await _dashboardService.GetDashboardDataAsync(days);
        await _dashboardService.KiemTraVaTaoCanhBaoAsync();
        return View(dashboard);
    }
}

