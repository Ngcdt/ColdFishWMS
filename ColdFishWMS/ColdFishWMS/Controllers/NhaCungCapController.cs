using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;
using ColdFishWMS.Models;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Business.Services;
using System.Security.Claims;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
public class NhaCungCapController : Controller
{
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public NhaCungCapController(ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _context = context;
        _systemLogService = systemLogService;
    }

    // GET: NhaCungCap
    public async Task<IActionResult> Index(string searchString)
    {
        var query = _context.NhaCungCaps.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(s => s.TenNhaCungCap.Contains(searchString) 
                                  || s.MaNhaCungCap.Contains(searchString)
                                  || s.DiaChi.Contains(searchString)
                                  || s.SoDienThoai.Contains(searchString)
                                  || s.Email.Contains(searchString));
        }

        ViewBag.SearchString = searchString;
        return View(await query.OrderByDescending(x => x.MaNhaCungCap).ToListAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Create(NhaCungCap nhaCungCap)
    {
        if (ModelState.IsValid)
        {
            _context.Add(nhaCungCap);
            await _context.SaveChangesAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Create", $"Thêm nhà cung cấp: {nhaCungCap.TenNhaCungCap}", userId, "NhaCungCap", nhaCungCap.MaNhaCungCap, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Đã thêm nhà cung cấp thành công!";
            return RedirectToAction(nameof(Index));
        }
        TempData["Error"] = "Thêm mới thất bại. Vui lòng kiểm tra lại thông tin.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(string id, NhaCungCap nhaCungCap)
    {
        if (id != nhaCungCap.MaNhaCungCap)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(nhaCungCap);
                await _context.SaveChangesAsync();

                // Logging
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (int.TryParse(userIdStr, out int uid)) userId = uid;
                await _systemLogService.LogAsync("Update", $"Cập nhật nhà cung cấp: {nhaCungCap.TenNhaCungCap}", userId, "NhaCungCap", nhaCungCap.MaNhaCungCap, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = "Cập nhật nhà cung cấp thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhaCungCapExists(nhaCungCap.MaNhaCungCap))
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
        TempData["Error"] = "Cập nhật thất bại.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(string id)
    {
        var nhaCungCap = await _context.NhaCungCaps.FindAsync(id);
        if (nhaCungCap != null)
        {
            // Soft delete usually? But for now let's do soft delete logic if supported, or Hard. 
            // Entity has TrangThaiHoatDong.
            nhaCungCap.TrangThaiHoatDong = false;
            _context.Update(nhaCungCap);
            await _context.SaveChangesAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Delete", $"Xóa (ẩn) nhà cung cấp: {nhaCungCap.TenNhaCungCap}", userId, "NhaCungCap", id, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Đã xóa (ẩn) nhà cung cấp.";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool NhaCungCapExists(string id)
    {
        return _context.NhaCungCaps.Any(e => e.MaNhaCungCap == id);
    }
}
