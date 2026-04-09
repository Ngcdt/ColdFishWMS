using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho + "," + ColdFishWMS.Models.AppRoles.NhanVienKho + "," + ColdFishWMS.Models.AppRoles.KeToanKho)]
public class LoaiSanPhamController : Controller
{
    private readonly ColdFishDbContext _context;

    public LoaiSanPhamController(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.LoaiSanPhams.ToListAsync());
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Create(LoaiSanPham loai)
    {
        if (ModelState.IsValid)
        {
            _context.Add(loai);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm mới loại sản phẩm thành công";
            return RedirectToAction(nameof(Index));
        }
        return View(loai);
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var loai = await _context.LoaiSanPhams.FindAsync(id);
        if (loai == null) return NotFound();
        return View(loai);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Edit(int id, LoaiSanPham loai)
    {
        if (id != loai.MaLoai) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(loai);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thành công";
            return RedirectToAction(nameof(Index));
        }
        return View(loai);
    }

    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var loai = await _context.LoaiSanPhams.FindAsync(id);
        if (loai != null)
        {
             // Check usage
             var used = await _context.SanPhams.AnyAsync(s => s.MaLoai == id);
             if(used)
             {
                 TempData["Error"] = "Không thể xóa loại đang được sử dụng bởi sản phẩm khác!";
                 return RedirectToAction(nameof(Index));
             }

            _context.LoaiSanPhams.Remove(loai);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa thành công";
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return Json(await _context.LoaiSanPhams.ToListAsync());

        var results = await _context.LoaiSanPhams
            .Where(l => l.TenLoai.Contains(keyword))
            .ToListAsync();
        return Json(results);
    }
}
