using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;

using ColdFishWMS.Models;
using System.Security.Claims;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho + "," + AppRoles.KeToanKho)]
public class SanPhamController : Controller
{
    private readonly ISanPhamService _sanPhamService;
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public SanPhamController(ISanPhamService sanPhamService, ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _sanPhamService = sanPhamService;
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index()
    {
        var sanPhams = await _sanPhamService.GetAllAsync();
        return View(sanPhams);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.DonViTinhs = _context.DonViTinhs.ToList();
        ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
        ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(x => x.TrangThaiHoatDong).ToList();
        return View();
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SanPham model)
    {
        if (ModelState.IsValid)
        {
            await _sanPhamService.CreateAsync(model);
            
            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int id)) userId = id;
            await _systemLogService.LogAsync("Create", $"Thêm mới sản phẩm: {model.TenSanPham}", userId, "SanPham", model.MaSanPham, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        ViewBag.DonViTinhs = _context.DonViTinhs.ToList();
        ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
        ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(x => x.TrangThaiHoatDong).ToList();
        return View(model);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var sanPham = await _sanPhamService.GetByIdAsync(id);
        if (sanPham == null)
            return NotFound();

        ViewBag.DonViTinhs = _context.DonViTinhs.ToList();
        ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
        ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(x => x.TrangThaiHoatDong).ToList();
        return View(sanPham);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SanPham model)
    {
        if (ModelState.IsValid)
        {
            await _sanPhamService.UpdateAsync(model);

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int id)) userId = id;
            await _systemLogService.LogAsync("Update", $"Cập nhật sản phẩm: {model.TenSanPham}", userId, "SanPham", model.MaSanPham, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Index");
        }

        ViewBag.DonViTinhs = _context.DonViTinhs.ToList();
        ViewBag.LoaiSanPhams = _context.LoaiSanPhams.ToList();
        ViewBag.NhaCungCaps = _context.NhaCungCaps.Where(x => x.TrangThaiHoatDong).ToList();
        return View(model);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _sanPhamService.DeleteAsync(id);
        
        if (result)
        {
            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Delete", $"Xóa sản phẩm: {id}", userId, "SanPham", id, HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        TempData[result ? "Success" : "Error"] = result ? "Xóa sản phẩm thành công!" : "Không thể xóa sản phẩm!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(string keyword)
    {
        var results = await _sanPhamService.SearchAsync(keyword);
        return Json(results);
    }
}

