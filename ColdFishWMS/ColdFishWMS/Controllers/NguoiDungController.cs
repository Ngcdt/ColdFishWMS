using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Models;
using ColdFishWMS.Models.ViewModels;
using ColdFishWMS.Data;
using ColdFishWMS.Data;
using System.Security.Claims;
using ColdFishWMS.Business.Services;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = AppRoles.QuanLyKho)]
public class NguoiDungController : Controller
{
    private readonly ColdFishDbContext _context;
    private readonly ISystemLogService _systemLogService;

    public NguoiDungController(ColdFishDbContext context, ISystemLogService systemLogService)
    {
        _context = context;
        _systemLogService = systemLogService;
    }

    public async Task<IActionResult> Index(string search = "", int? roleId = null, string status = "")
    {
        // 1. Thống kê
        var tongNguoiDung = await _context.NguoiDungs.CountAsync();
        var nguoiDungHoatDong = await _context.NguoiDungs.CountAsync(x => x.TrangThaiHoatDong);
        var soLuongVaiTro = await _context.VaiTros.CountAsync();
        var dangNhapHomNay = 0; // Property LanDangNhapCuoi removed

        // 2. Query Users
        var query = _context.NguoiDungs.Include(x => x.VaiTro).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower().Trim();
            query = query.Where(x => x.HoTen.ToLower().Contains(search) || 
                                     x.Email.ToLower().Contains(search) || 
                                     x.TenDangNhap.ToLower().Contains(search));
        }

        if (roleId.HasValue && roleId.Value > 0)
        {
            query = query.Where(x => x.MaVaiTro == roleId);
        }

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "active") query = query.Where(x => x.TrangThaiHoatDong);
            if (status == "inactive") query = query.Where(x => !x.TrangThaiHoatDong);
        }

        var users = await query.OrderByDescending(x => x.MaNguoiDung).ToListAsync();

        // 3. Prepare ViewModel
        var model = new UserManagementViewModel
        {
            TongNguoiDung = tongNguoiDung,
            NguoiDungHoatDong = nguoiDungHoatDong,
            SoLuongVaiTro = soLuongVaiTro,
            DangNhapHomNay = dangNhapHomNay,
            DanhSachNguoiDung = users,
            TuKhoa = search,
            MaVaiTroFilter = roleId
        };

        // 4. ViewBag for Dropdowns
        ViewBag.VaiTros = await _context.VaiTros.ToListAsync();

        return View(model);
    }
    public async Task<IActionResult> Create()
    {
        ViewBag.VaiTros = await _context.VaiTros.ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ColdFishWMS.Models.Entities.NguoiDung user)
    {
        // Loại bỏ validate cho các trường tự sinh hoặc null
        ModelState.Remove("VaiTro");
        
        if (ModelState.IsValid)
        {
            // Chuẩn hóa dữ liệu
            user.TenDangNhap = user.TenDangNhap?.Trim();
            user.Email = user.Email?.Trim();

            // Kiểm tra trùng username
            if (await _context.NguoiDungs.AnyAsync(x => x.TenDangNhap == user.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại");
                ViewBag.VaiTros = await _context.VaiTros.ToListAsync();
                return View(user);
            }

            // Hash password
            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(user.MatKhau);
            user.NgayTao = DateTime.Now;
            user.SoLanSai = 0;
            user.TrangThai = user.TrangThaiHoatDong ? "Hoạt động" : "Bị khóa";

            _context.Add(user);
            await _context.SaveChangesAsync();

            // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Create", $"Tạo người dùng mới: {user.TenDangNhap}", userId, "NguoiDung", user.MaNguoiDung.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());

            return RedirectToAction(nameof(Index));
        }

        ViewBag.VaiTros = await _context.VaiTros.ToListAsync();
        return View(user);
    }
    // --- DETAILS ---
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(m => m.MaNguoiDung == id);

        if (user == null) return NotFound();

        return View(user);
    }

    // --- EDIT ---
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.NguoiDungs.FindAsync(id);
        if (user == null) return NotFound();

        ViewBag.VaiTros = await _context.VaiTros.ToListAsync();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ColdFishWMS.Models.Entities.NguoiDung user, string? NewPassword)
    {
        if (id != user.MaNguoiDung) return NotFound();

        // Remove validations for fields we handle manually
        ModelState.Remove("VaiTro");
        ModelState.Remove("MatKhau"); 

        if (ModelState.IsValid)
        {
            try
            {
                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null) return NotFound();

                // Update Info
                existingUser.HoTen = user.HoTen;
                existingUser.Email = user.Email;
                existingUser.SoDienThoai = user.SoDienThoai;
                existingUser.MaVaiTro = user.MaVaiTro;
                existingUser.TrangThaiHoatDong = user.TrangThaiHoatDong;
                existingUser.TrangThai = user.TrangThaiHoatDong ? "Hoạt động" : "Bị khóa";
                existingUser.NgayCapNhat = DateTime.Now;

                // Handle Password
                if (!string.IsNullOrEmpty(NewPassword))
                {
                    existingUser.MatKhau = BCrypt.Net.BCrypt.HashPassword(NewPassword);
                }

                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                
                await _context.SaveChangesAsync();
                
                // Logging
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = null;
                if (int.TryParse(userIdStr, out int uid)) userId = uid;
                await _systemLogService.LogAsync("Update", $"Cập nhật người dùng: {existingUser.TenDangNhap}", userId, "NguoiDung", existingUser.MaNguoiDung.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.NguoiDungs.AnyAsync(e => e.MaNguoiDung == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        ViewBag.VaiTros = await _context.VaiTros.ToListAsync();
        return View(user);
    }

    // --- DELETE ---
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(m => m.MaNguoiDung == id);

        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.NguoiDungs.FindAsync(id);
        if (user != null)
        {
            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();

             // Logging
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = null;
            if (int.TryParse(userIdStr, out int uid)) userId = uid;
            await _systemLogService.LogAsync("Delete", $"Xóa người dùng: {user.TenDangNhap}", userId, "NguoiDung", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());
        }
        return RedirectToAction(nameof(Index));
    }
}
