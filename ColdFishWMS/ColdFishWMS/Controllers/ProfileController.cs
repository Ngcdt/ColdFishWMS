using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ColdFishDbContext _context;

    public ProfileController(ColdFishDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Auth");

        var user = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .FirstOrDefaultAsync(u => u.MaNguoiDung == userId);

        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string HoTen, string Email, string SoDienThoai)
    {
        var userId = GetCurrentUserId();
        var user = await _context.NguoiDungs.FindAsync(userId);

        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(HoTen))
        {
            TempData["Error"] = "Họ tên không được để trống";
            return RedirectToAction("Index");
        }

        user.HoTen = HoTen.Trim();
        user.Email = Email?.Trim();
        user.SoDienThoai = SoDienThoai?.Trim();
        user.NgayCapNhat = DateTime.Now;

        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật thông tin thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
    {
        var userId = GetCurrentUserId();
        var user = await _context.NguoiDungs.FindAsync(userId);

        if (user == null) return NotFound();

        if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword))
        {
            TempData["PassError"] = "Vui lòng nhập đầy đủ thông tin";
            return RedirectToAction("Index");
        }

        if (NewPassword != ConfirmPassword)
        {
             TempData["PassError"] = "Mật khẩu xác nhận không khớp";
             return RedirectToAction("Index");
        }

        if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.MatKhau))
        {
             TempData["PassError"] = "Mật khẩu hiện tại không đúng";
             return RedirectToAction("Index");
        }

        user.MatKhau = BCrypt.Net.BCrypt.HashPassword(NewPassword);
        user.NgayCapNhat = DateTime.Now;

        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["PassSuccess"] = "Đổi mật khẩu thành công!";
        return RedirectToAction("Index");
    }
}
