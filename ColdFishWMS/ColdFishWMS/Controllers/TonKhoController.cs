using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;

namespace ColdFishWMS.Controllers;

[Authorize]
public class TonKhoController : Controller
{
    private readonly ColdFishDbContext _context;

    public TonKhoController(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search)
    {
        var query = _context.LoHangs
            .Include(l => l.SanPham)
                .ThenInclude(s => s.DonViTinh)
            .Include(l => l.ViTriKho)
            .Where(l => l.SoLuongTon > 0);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l => 
                l.MaLoHang.Contains(search) || 
                l.SanPham.TenSanPham.Contains(search) ||
                l.SanPham.MaSanPham.Contains(search));
        }

        var tonKho = await query.OrderBy(l => l.HanSuDung).ToListAsync();
        ViewBag.CurrentSearch = search;

        return View(tonKho);
    }

    [HttpGet]
    public async Task<IActionResult> TraCuu(string keyword)
    {
        var query = _context.LoHangs
            .Include(l => l.SanPham)
                .ThenInclude(s => s.DonViTinh)
            .Include(l => l.ViTriKho)
            .Where(l => l.SoLuongTon > 0);

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(l =>
                l.SanPham!.TenSanPham.Contains(keyword) ||
                l.SanPham.MaSanPham.Contains(keyword) ||
                l.MaLoHang.Contains(keyword));
        }

        var results = await query.OrderBy(l => l.HanSuDung).ToListAsync();
        return Json(results);
    }
}

