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
public class KhachHangController : Controller
{
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public KhachHangController(ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.KhachHangs.ToListAsync());
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Create(KhachHang khachHang)
    {
        if (ModelState.IsValid)
        {
            if (await _context.KhachHangs.AnyAsync(x => x.MaKhachHang == khachHang.MaKhachHang))
            {
                ModelState.AddModelError("MaKhachHang", "Mã khách hàng đã tồn tại.");
                return View(khachHang);
            }

            _context.Add(khachHang);
            await _context.SaveChangesAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Create", $"Thêm khách hàng: {khachHang.TenKhachHang}", userId, "KhachHang", khachHang.MaKhachHang, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Thêm mới khách hàng thành công";
            return RedirectToAction(nameof(Index));
        }
        return View(khachHang);
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var khachHang = await _context.KhachHangs.FindAsync(id);
        if (khachHang == null) return NotFound();
        return View(khachHang);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(string id, KhachHang khachHang)
    {
        if (id != khachHang.MaKhachHang) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(khachHang);
                await _context.SaveChangesAsync();

                // Logging
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (int.TryParse(userIdStr, out int uid)) userId = uid;
                await _systemLogService.LogAsync("Update", $"Cập nhật khách hàng: {khachHang.TenKhachHang}", userId, "KhachHang", khachHang.MaKhachHang, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["Success"] = "Cập nhật thành công";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhachHangExists(khachHang.MaKhachHang)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(khachHang);
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();
        var khachHang = await _context.KhachHangs.FindAsync(id);
        if (khachHang != null)
        {
            // Check usage
             var used = await _context.PhieuXuats.AnyAsync(p => p.MaKhachHang == id);
             if (used)
             {
                 TempData["Error"] = "Không thể xóa khách hàng đã có giao dịch xuất kho!";
                 return RedirectToAction(nameof(Index));
             }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Delete", $"Xóa khách hàng: {khachHang.TenKhachHang}", userId, "KhachHang", id, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Xóa khách hàng thành công";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool KhachHangExists(string id)
    {
        return _context.KhachHangs.Any(e => e.MaKhachHang == id);
    }
}
