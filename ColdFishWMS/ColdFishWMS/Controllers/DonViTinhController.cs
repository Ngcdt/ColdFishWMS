using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;
using ColdFishWMS.Models;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho + "," + ColdFishWMS.Models.AppRoles.KeToanKho)]
public class DonViTinhController : Controller
{
    private readonly ColdFishDbContext _context;

    public DonViTinhController(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.DonViTinhs.ToListAsync();
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Create(DonViTinh dvt)
    {
        if (ModelState.IsValid)
        {
            // Check duplicate name
            if (await _context.DonViTinhs.AnyAsync(d => d.TenDonViTinh == dvt.TenDonViTinh))
            {
                TempData["Error"] = "Tên đơn vị tính đã tồn tại.";
            }
            else
            {
                _context.Add(dvt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm đơn vị tính thành công.";
            }
        }
        else
        {
             TempData["Error"] = "Dữ liệu không hợp lệ.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(int id, DonViTinh dvt)
    {
        if (id != dvt.MaDonViTinh) return NotFound();

        if (ModelState.IsValid)
        {
             try
            {
                _context.Update(dvt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DonViTinhs.Any(e => e.MaDonViTinh == id)) return NotFound();
                else throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(int id)
    {
        var dvt = await _context.DonViTinhs.FindAsync(id);
        if (dvt != null)
        {
            // Check usage
            var isUsed = await _context.SanPhams.AnyAsync(s => s.MaDonViTinh == id);
            if (isUsed)
            {
                TempData["Error"] = "Không thể xóa đơn vị tính đang được sử dụng bởi sản phẩm.";
            }
            else
            {
                _context.DonViTinhs.Remove(dvt);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa đơn vị tính.";
            }
        }
        return RedirectToAction(nameof(Index));
    }
}
