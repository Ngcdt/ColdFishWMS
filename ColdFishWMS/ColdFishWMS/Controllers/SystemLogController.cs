using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Models;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = AppRoles.QuanLyKho)]
public class SystemLogController : Controller
{
    private readonly ISystemLogService _systemLogService;

    public SystemLogController(ISystemLogService systemLogService)
    {
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        int pageSize = 50;
        var logs = await _systemLogService.GetLogsAsync(page, pageSize, search);
        
        ViewBag.CurrentPage = page;
        ViewBag.CurrentSearch = search;
        
        // Simple Next Page check
        ViewBag.HasNextPage = logs.Count == pageSize;
        ViewBag.HasPreviousPage = page > 1;

        return View(logs);
    }
}
