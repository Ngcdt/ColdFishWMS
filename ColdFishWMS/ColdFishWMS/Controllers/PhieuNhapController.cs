using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Data;
using System.Security.Claims;

using ColdFishWMS.Models;

namespace ColdFishWMS.Controllers;

[Authorize]
public class PhieuNhapController : Controller
{
    private readonly IPhieuNhapService _phieuNhapService;
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public PhieuNhapController(IPhieuNhapService phieuNhapService, ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _phieuNhapService = phieuNhapService;
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index()
    {
        var phieuNhaps = await _phieuNhapService.GetAllAsync();
        return View(phieuNhaps);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public IActionResult Create()
    {
        ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(n => n.TrangThaiHoatDong).ToList();
        ViewBag.SanPhams = _context.SanPhams.Include(s => s.DonViTinh).Where(s => s.TrangThaiHoatDong).ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public async Task<IActionResult> Create(PhieuNhap phieuNhap, List<ColdFishWMS.Models.DTOs.PhieuNhapItemDto> chiTietList)
    {
        try
        {
            // 1. Validate List
            if (chiTietList == null || !chiTietList.Any())
            {
                TempData["Error"] = "Vui lòng thêm ít nhất một sản phẩm!";
                ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(n => n.TrangThaiHoatDong).ToList();
                ViewBag.SanPhams = _context.SanPhams.Include(s => s.DonViTinh).Where(s => s.TrangThaiHoatDong).ToList();
                return View(phieuNhap);
            }
            
            // ... (rest of method implicit logic, just matching start to ensure replacement)
            // Actually, replace_file_content is block replacement. I need to be careful not to delete the body.
            // I will target the header area specifically.


            // 2. Validate User
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId))
            {
                 TempData["Error"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                 return RedirectToAction("Login", "Auth");
            }
            phieuNhap.MaNguoiTao = int.Parse(claimId);
            
            // Fix: Append current time to selected date to avoid 00:00
            phieuNhap.NgayNhap = phieuNhap.NgayNhap.Date.Add(DateTime.Now.TimeOfDay);

            // 3. Map DTO to Entity List
            var entities = chiTietList.Select(dto => new ChiTietPhieuNhap
            {
                MaSanPham = dto.MaSanPham,
                SoLuong = dto.SoLuong,
                DonGia = dto.DonGia,
                MaLoHang = null, // Defer to Receiving step
                LoHang = null
            }).ToList();

            await _phieuNhapService.CreateAsync(phieuNhap, entities);
            
            // Logging
            await _systemLogService.LogAsync("Create", $"Tạo phiếu nhập mới: {phieuNhap.MaPhieuNhap}", phieuNhap.MaNguoiTao, "PhieuNhap", phieuNhap.MaPhieuNhap, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Tạo phiếu nhập thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null) msg += $" -> {ex.InnerException.Message}";
            
            TempData["Error"] = $"Lỗi: {msg}";
            
            ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(n => n.TrangThaiHoatDong).ToList();
            ViewBag.SanPhams = _context.SanPhams.Include(s => s.DonViTinh).Where(s => s.TrangThaiHoatDong).ToList();
            return View(phieuNhap);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();
        var phieuNhap = await _phieuNhapService.GetByIdAsync(id);
        if (phieuNhap == null) return NotFound();
        return View(phieuNhap);
    }

    public async Task<IActionResult> Print(string id)
    {
        if (id == null) return NotFound();
        var phieuNhap = await _phieuNhapService.GetByIdAsync(id);
        if (phieuNhap == null) return NotFound();
        return View(phieuNhap);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.QuanLyKho)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _phieuNhapService.DeleteAsync(id);
        
        if (result)
        {
             // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Delete", $"Xóa phiếu nhập: {id}", userId, "PhieuNhap", id, HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        TempData[result ? "Success" : "Error"] = result ? "Xóa phiếu nhập thành công!" : "Không thể xóa phiếu nhập đã duyệt!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public async Task<IActionResult> NhapKho(string id)
    {
        if (id == null) return NotFound();

        var phieuNhap = await _context.PhieuNhaps
            .Include(p => p.NhaCungCap)
            .Include(p => p.NguoiTao)
            .Include(p => p.ChiTietPhieuNhaps)
                .ThenInclude(ct => ct.SanPham)
                    .ThenInclude(sp => sp.DonViTinh)
            .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);

        if (phieuNhap == null) return NotFound();
        if (phieuNhap.DaDuyet)
        {
            TempData["Error"] = "Phiếu nhập này đã được nhập kho!";
            return RedirectToAction("Details", new { id = id });
        }

        ViewBag.ViTriKhos = await _context.ViTriKhos.ToListAsync();
        return View(phieuNhap);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NhapKho(string id, List<ChiTietPhieuNhap> chiTietList)
    {
        var phieuNhap = await _context.PhieuNhaps
            .Include(p => p.ChiTietPhieuNhaps)
            .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);

        if (phieuNhap == null) return NotFound();
        if (phieuNhap.DaDuyet) return RedirectToAction("Index");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var itemInput in chiTietList)
            {
                var existingItem = phieuNhap.ChiTietPhieuNhaps.FirstOrDefault(x => x.MaChiTiet == itemInput.MaChiTiet);
                if (existingItem != null)
                {
                    // Update Lot/Location info
                    existingItem.MaLoHang = itemInput.MaLoHang;
                    
                    if (string.IsNullOrEmpty(itemInput.MaLoHang)) continue; 

                    var existingLoHang = await _context.LoHangs.FindAsync(itemInput.MaLoHang);
                    if (existingLoHang == null)
                    {
                        var newLoHang = new LoHang
                        {
                            MaLoHang = itemInput.MaLoHang,
                            MaSanPham = existingItem.MaSanPham,
                            NgaySanXuat = itemInput.LoHang?.NgaySanXuat ?? DateTime.Now,
                            HanSuDung = itemInput.LoHang?.HanSuDung ?? DateTime.Now.AddYears(1),
                            SoLuongNhap = existingItem.SoLuong,
                            SoLuongTon = existingItem.SoLuong,
                            MaViTri = itemInput.LoHang?.MaViTri,
                            NgayTao = DateTime.Now
                        };
                        _context.LoHangs.Add(newLoHang);
                    }
                    else
                    {
                         // Update existing lot quantity
                        existingLoHang.SoLuongNhap += existingItem.SoLuong;
                        existingLoHang.SoLuongTon += existingItem.SoLuong;
                        _context.LoHangs.Update(existingLoHang);
                    }
                }
            }

            phieuNhap.DaDuyet = true;
            _context.PhieuNhaps.Update(phieuNhap);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("NhapKho", $"Đã nhập kho phiếu: {id}", userId, "PhieuNhap", id, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Nhập kho thành công!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi khi nhập kho: " + ex.Message;
            return RedirectToAction("NhapKho", new { id = id });
        }
    }
}

