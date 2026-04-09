    // Placeholder action because I didn't read PhieuXuatController file content explicitly in previous step but I assumed it has Details/Delete similar to PhieuNhap based on context. 
    // Wait, I listed PhieuXuatController in step 224 but didn't view it in step 229, 230, 231.
    // I viewed PhieuNhapController.
    // I should view PhieuXuatController first to be safe!
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Data;
using System.Security.Claims;

using ColdFishWMS.Models;

namespace ColdFishWMS.Controllers;

[Authorize]
public class PhieuXuatController : Controller
{
    private readonly IPhieuXuatService _phieuXuatService;
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public PhieuXuatController(IPhieuXuatService phieuXuatService, ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _phieuXuatService = phieuXuatService;
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index()
    {
        var phieuXuats = await _phieuXuatService.GetAllAsync();
        return View(phieuXuats);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public IActionResult Create()
    {
        ViewBag.KhachHangs = _context.KhachHangs.Where(k => k.TrangThaiHoatDong).ToList();
        ViewBag.SanPhams = _context.SanPhams
            .Include(s => s.DonViTinh)
            .Include(s => s.LoHangs.Where(l => l.SoLuongTon > 0))
            .Where(s => s.TrangThaiHoatDong)
            .ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public async Task<IActionResult> Create(PhieuXuat phieuXuat, List<ColdFishWMS.Models.DTOs.PhieuXuatItemDto> danhSachXuat)
    {
        try
        {
            var maNguoiDung = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            phieuXuat.MaNguoiTao = maNguoiDung;

            // Fix: Append current time to selected date to avoid 00:00
            phieuXuat.NgayXuat = phieuXuat.NgayXuat.Date.Add(DateTime.Now.TimeOfDay);

            await _phieuXuatService.CreateWithFEFOAsync(phieuXuat, danhSachXuat);
            
            // Logging
            await _systemLogService.LogAsync("Create", $"Tạo phiếu xuất mới: {phieuXuat.MaPhieuXuat}", maNguoiDung, "PhieuXuat", phieuXuat.MaPhieuXuat, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Tạo phiếu xuất thành công với FEFO!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi: {ex.Message}";
            ViewBag.KhachHangs = _context.KhachHangs.Where(k => k.TrangThaiHoatDong).ToList();
            ViewBag.SanPhams = _context.SanPhams
                .Include(s => s.DonViTinh)
                .Include(s => s.LoHangs.Where(l => l.SoLuongTon > 0))
                .Where(s => s.TrangThaiHoatDong)
                .ToList();
            return View(phieuXuat);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho)]
    public async Task<IActionResult> GetDeXuatFEFO(string maSanPham, decimal soLuong)
    {
        var deXuat = await _phieuXuatService.GetDeXuatFEFOAsync(maSanPham, soLuong);
        return Json(deXuat.Select(l => new
        {
            l.MaLoHang,
            SoLo = l.MaLoHang, // SoLo removed, use MaLoHang
            l.HanSuDung,
            l.SoLuongTon,
            ViTri = l.ViTriKho?.MaViTri,
            SoNgayConLai = (l.HanSuDung - DateTime.Now).Days
        }));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var phieuXuat = await _phieuXuatService.GetByIdAsync(id);
        if (phieuXuat == null)
            return NotFound();

        return View(phieuXuat);
    }

    public async Task<IActionResult> Print(string id)
    {
        if (id == null) return NotFound();
        var phieuXuat = await _phieuXuatService.GetByIdAsync(id);
        if (phieuXuat == null) return NotFound();
        return View(phieuXuat);
    }
}

