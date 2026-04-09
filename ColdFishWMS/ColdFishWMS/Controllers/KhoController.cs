using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Controllers;

[Authorize]
public class KhoController : Controller
{
    private readonly ColdFishDbContext _context;

    public KhoController(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, string location, string status)
    {
        // 1. Calculate Stats (Global)
        var allBatchesKey = await _context.LoHangs
            .Include(l => l.SanPham)
            .Include(l => l.ViTriKho)
            .ToListAsync();

        var totalItems = allBatchesKey.Sum(l => l.SoLuongTon);
        var totalValue = allBatchesKey.Sum(l => l.SoLuongTon * (l.SanPham?.GiaNhapMacDinh ?? 0));
        var expiringSoon = allBatchesKey.Count(l => (l.HanSuDung - DateTime.Today).TotalDays <= 30 && l.SoLuongTon > 0);
        var totalBatches = allBatchesKey.Count;

        ViewBag.TotalItems = totalItems;
        ViewBag.TotalValue = totalValue;
        ViewBag.ExpiringSoon = expiringSoon;
        ViewBag.TotalBatches = totalBatches;

        // Populate Locations for Dropdown
        ViewBag.Locations = await _context.ViTriKhos.OrderBy(v => v.MaViTri).ToListAsync();

        // Retain Filter Values
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentLocation = location;
        ViewBag.CurrentStatus = status;

        // 2. Filter Logic
        var query = _context.LoHangs
            .Include(l => l.SanPham)
            .Include(l => l.ViTriKho)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(l => l.MaLoHang.ToLower().Contains(search) || 
                                     l.SanPham.TenSanPham.ToLower().Contains(search) ||
                                     l.SanPham.MaSanPham.ToLower().Contains(search));
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(l => l.ViTriKho.MaViTri == location);
        }

        if (!string.IsNullOrEmpty(status))
        {
            var today = DateTime.Today;
            if (status == "valid") // Còn hạn (> 30 ngày)
                query = query.Where(l => EF.Functions.DateDiffDay(today, l.HanSuDung) >= 30);
            else if (status == "expiring") // Sắp hết hạn (0 <= days < 30)
                query = query.Where(l => EF.Functions.DateDiffDay(today, l.HanSuDung) < 30 && EF.Functions.DateDiffDay(today, l.HanSuDung) >= 0);
            else if (status == "expired") // Hết hạn (< 0)
                query = query.Where(l => l.HanSuDung < today);
        }

        // Return sorted list
        var result = await query.OrderBy(l => l.HanSuDung).ToListAsync();

        return View(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickImport(string SpCode, string MaLo, decimal SoLuong, DateTime HanSuDung, string ViTriCode)
    {
        try
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == User.Identity.Name);
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.MaSanPham == SpCode)
                  ?? await _context.SanPhams.FirstOrDefaultAsync(s => s.TenSanPham == SpCode); // Try Name too
            var vt = await _context.ViTriKhos.FirstOrDefaultAsync(v => v.MaViTri == ViTriCode);

            if (sp == null) throw new Exception("Không tìm thấy sản phẩm này!");
            if (vt == null) throw new Exception("Mã vị trí không hợp lệ!");
            if (string.IsNullOrEmpty(MaLo)) throw new Exception("Mã lô không thể trống!");

            // Create Ticket
            var pn = new PhieuNhap
            {
                MaPhieuNhap = $"PN-QUICK-{DateTime.Now:HHmmss}",
                NgayNhap = DateTime.Now,
                MaNguoiTao = user?.MaNguoiDung ?? 1,
                GhiChu = "Nhập kho nhanh",
                DaDuyet = true, // Auto approve
                NgayTao = DateTime.Now
            };
            _context.PhieuNhaps.Add(pn);
            await _context.SaveChangesAsync();

            // Create Batch
            var lo = new LoHang
            {
                MaLoHang = MaLo,
                MaSanPham = sp.MaSanPham,
                NgaySanXuat = DateTime.Now, // Default
                HanSuDung = HanSuDung,
                SoLuongNhap = SoLuong,
                SoLuongTon = SoLuong,
                MaViTri = vt.MaViTri,
                NgayTao = DateTime.Now
            };
            _context.LoHangs.Add(lo);
            await _context.SaveChangesAsync();

            // Detail
            var ct = new ChiTietPhieuNhap
            {
                MaPhieuNhap = pn.MaPhieuNhap,
                MaSanPham = sp.MaSanPham,
                MaLoHang = lo.MaLoHang,
                SoLuong = SoLuong,
                DonGia = sp.GiaNhapMacDinh
            };
            _context.ChiTietPhieuNhaps.Add(ct);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã nhập nhanh lô {MaLo} vào vị trí {ViTriCode}";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi nhập kho: " + ex.Message;
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickExport(string SpCode, decimal SoLuong)
    {
        try
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.TenDangNhap == User.Identity.Name);
            var sp = await _context.SanPhams.FirstOrDefaultAsync(s => s.MaSanPham == SpCode);
            if (sp == null) throw new Exception("Sản phẩm không tồn tại");

            // FEFO Logic simplified
            var batches = await _context.LoHangs
                .Where(l => l.MaSanPham == sp.MaSanPham && l.SoLuongTon > 0)
                .OrderBy(l => l.HanSuDung)
                .ToListAsync();

            if (batches.Sum(b => b.SoLuongTon) < SoLuong)
                throw new Exception("Không đủ tồn kho!");

            var ao = new PhieuXuat
            {
                MaPhieuXuat = $"PX-QUICK-{DateTime.Now:HHmmss}",
                NgayXuat = DateTime.Now,
                MaNguoiTao = user?.MaNguoiDung ?? 1,
                GhiChu = "Xuất kho nhanh (FEFO)",
                DaXuat = true,
                NgayTao = DateTime.Now
            };
            _context.PhieuXuats.Add(ao);
            await _context.SaveChangesAsync();

            decimal remaining = SoLuong;
            foreach (var b in batches)
            {
                if (remaining <= 0) break;
                decimal take = Math.Min(remaining, b.SoLuongTon);

                b.SoLuongTon -= take;
                remaining -= take;

                _context.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
                {
                    MaPhieuXuat = ao.MaPhieuXuat,
                    MaSanPham = sp.MaSanPham,
                    MaLoHang = b.MaLoHang,
                    SoLuong = take,
                    DonGia = sp.GiaNhapMacDinh
                });
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xuất kho nhanh thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi xuất kho: " + ex.Message;
        }
        return RedirectToAction("Index");
    }
}
