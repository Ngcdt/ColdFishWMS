using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColdFishWMS.Data;
using ColdFishWMS.Models;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho + "," + ColdFishWMS.Models.AppRoles.NhanVienKho + "," + ColdFishWMS.Models.AppRoles.KeToanKho)]
public class CanhBaoController : Controller
{
    private readonly ColdFishDbContext _context;
    private readonly IAlertConfigService _configService;

    public CanhBaoController(ColdFishDbContext context, IAlertConfigService configService)
    {
        _context = context;
        _configService = configService;
    }

    // Dashboard Cảnh báo
    public async Task<IActionResult> Index(bool refresh = false)
    {
        if (refresh)
        {
             TempData["RefreshSuccess"] = true;
        }

        await ScanForAlerts(); // Auto-scan on load

        var activeAlerts = await _context.CanhBaos
            .Include(c => c.SanPham)
            .Include(c => c.LoHang)
            .Where(c => !c.DaXuLy)
            .OrderByDescending(c => c.MucDo == "Nguy hiểm").ThenByDescending(c => c.NgayTao)
            .ToListAsync();

        return View(activeAlerts);
    }

    public async Task<IActionResult> History()
    {
        var history = await _context.CanhBaos
            .Include(c => c.SanPham)
            .Include(c => c.LoHang)
            .Where(c => c.DaXuLy)
            .OrderByDescending(c => c.NgayXuLy)
            .ToListAsync();

        return View("Index", history); // Re-use Index view but with processed data? Or specific History view. Let's use Index view but pass a flag or handle it in View. Actually, let's just make Index handle both? Or separate.
        // Simplified: Return generic view, View decides title based on Context? 
        // For now, let's assume View handles a List<CanhBao>. 
        ViewBag.IsHistory = true;
        return View("Index", history);
    }

    [HttpPost]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho + "," + ColdFishWMS.Models.AppRoles.NhanVienKho)]
    public async Task<IActionResult> XuLy(int id)
    {
        var alert = await _context.CanhBaos.FindAsync(id);
        if (alert != null)
        {
            alert.DaXuLy = true;
            alert.NgayXuLy = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xử lý cảnh báo.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(int id)
    {
        var alert = await _context.CanhBaos.FindAsync(id);
        if (alert != null)
        {
            _context.CanhBaos.Remove(alert);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa cảnh báo.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> Settings()
    {
        var config = await _configService.GetConfigAsync();
        return View(config);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = ColdFishWMS.Models.AppRoles.QuanLyKho)]
    public async Task<IActionResult> SaveSettings(AlertConfigViewModel model)
    {
        if (ModelState.IsValid)
        {
            await _configService.SaveConfigAsync(model);
            TempData["Success"] = "Đã lưu cấu hình thành công!";
            return RedirectToAction(nameof(Settings));
        }
        return View("Settings", model);
    }

    private async Task ScanForAlerts()
    {
        var config = await _configService.GetConfigAsync();

        // 1. Check HSD
        // Warning if <= Config.ExpiryWarningDays
        var warningDate = DateTime.Today.AddDays(config.ExpiryWarningDays);

        var expiringBatches = await _context.LoHangs
            .Include(l => l.SanPham)
            .Where(l => l.SoLuongTon > 0 && l.HanSuDung <= warningDate) 
            .ToListAsync();

        foreach (var batch in expiringBatches)
        {
            var existing = await _context.CanhBaos.AnyAsync(c => c.MaLoHang == batch.MaLoHang && c.LoaiCanhBao == "Hạn sử dụng" && (!c.DaXuLy || c.NgayTao.Date == DateTime.Today));
            if (!existing)
            {
                var daysLeft = (batch.HanSuDung - DateTime.Today).Days;
                // Danger if < Config.ExpiryDangerDays (User: 3 days)
                var level = daysLeft <= config.ExpiryDangerDays ? "Nguy hiểm" : "Cảnh báo";
                var msg = daysLeft < 0 
                    ? $"Lô {batch.MaLoHang} ({batch.SanPham?.TenSanPham}) đã hết hạn {Math.Abs(daysLeft)} ngày!" 
                    : $"Lô {batch.MaLoHang} ({batch.SanPham?.TenSanPham}) sắp hết hạn trong {daysLeft} ngày.";

                _context.CanhBaos.Add(new CanhBao
                {
                    LoaiCanhBao = "Hạn sử dụng",
                    NoiDung = msg,
                    MaSanPham = batch.MaSanPham,
                    MaLoHang = batch.MaLoHang,
                    MucDo = level,
                    NgayTao = DateTime.Now
                });
            }
        }

        // 2. Check Low Stock
        var lowStocks = await _context.LoHangs
            .GroupBy(l => l.MaSanPham)
            .Select(g => new { MaSanPham = g.Key, TongTon = g.Sum(l => l.SoLuongTon) })
            .ToListAsync();

        foreach (var item in lowStocks)
        {
            var product = await _context.SanPhams.FindAsync(item.MaSanPham);
            // Use Product Threshold -> If 0/Default, use Global Config
            var threshold = (product != null && product.DinhMucTonThap > 0) ? product.DinhMucTonThap : config.MinStockThreshold;
            
            if (product != null && item.TongTon < threshold)
            {
                var existing = await _context.CanhBaos.AnyAsync(c => c.MaSanPham == item.MaSanPham && c.LoaiCanhBao == "Tồn kho" && (!c.DaXuLy || c.NgayTao.Date == DateTime.Today));
                if (!existing)
                {
                    _context.CanhBaos.Add(new CanhBao
                    {
                        LoaiCanhBao = "Tồn kho",
                        NoiDung = $"Sản phẩm '{product.TenSanPham}' còn {item.TongTon} (Dưới định mức {threshold}).",
                        MaSanPham = product.MaSanPham,
                        MucDo = "Cảnh báo",
                        NgayTao = DateTime.Now
                    });
                }
            }
        }

        // 3. Simulate Temp Alert (1 random if no active temp alerts)
        var hasTempAlert = await _context.CanhBaos.AnyAsync(c => c.LoaiCanhBao == "Nhiệt độ" && (!c.DaXuLy || c.NgayTao.Date == DateTime.Today));
        if (!hasTempAlert)
        {
            var randomProduct = await _context.SanPhams.FirstOrDefaultAsync();
            if (randomProduct != null)
            {
                 // Simulate a high temp event based on Config
                 // Example: If Max is -18, we simulate -10 (Too hot).
                 var simulatedTemp = config.MaxTemperature + 10; 

                 _context.CanhBaos.Add(new CanhBao
                {
                    LoaiCanhBao = "Nhiệt độ",
                    NoiDung = $"Nhiệt độ kho khu vực '{randomProduct.TenSanPham}': {simulatedTemp}°C (Vượt ngưỡng {config.MaxTemperature}°C).",
                    MaSanPham = randomProduct.MaSanPham,
                    MucDo = "Nguy hiểm",
                    NgayTao = DateTime.Now
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}
