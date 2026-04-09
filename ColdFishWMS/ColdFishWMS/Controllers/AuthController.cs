using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.DTOs;

namespace ColdFishWMS.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly INguoiDungRepository _userRepo;

    private readonly ISystemLogService _systemLogService;

    public AuthController(IAuthService authService, INguoiDungRepository userRepo, ISystemLogService systemLogService)
    {
        _authService = authService;
        _userRepo = userRepo;
        _systemLogService = systemLogService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var nguoiDung = await _authService.LoginAsync(model);

        if (nguoiDung == null)
        {
            // Debugging: Find out WHY it failed
            var debugUser = await _userRepo.GetByUsernameAsync(model.TenDangNhap?.Trim() ?? "");
            if (debugUser == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại (Kiểm tra lại tên đăng nhập).");
            }
            else if (!debugUser.TrangThaiHoatDong)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa hoặc chưa kích hoạt.");
            }
            else if (!BCrypt.Net.BCrypt.Verify(model.MatKhau, debugUser.MatKhau))
            {
                 ModelState.AddModelError("", "Mật khẩu không chính xác.");
            }
            else
            {
                ModelState.AddModelError("", "Đăng nhập thất bại (Lỗi không xác định).");
            }

            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
            new Claim(ClaimTypes.Name, nguoiDung.TenDangNhap),
            new Claim("HoTen", nguoiDung.HoTen),
            new Claim(ClaimTypes.Role, nguoiDung.VaiTro?.TenVaiTro ?? string.Empty),
            new Claim("MaVaiTro", nguoiDung.MaVaiTro.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.GhiNhoDangNhap,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        // LOGGING
        await _systemLogService.LogAsync("Login", $"Người dùng [{nguoiDung.TenDangNhap}] đăng nhập thành công.", nguoiDung.MaNguoiDung, "NguoiDung", nguoiDung.MaNguoiDung.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // LOGGING (Try to get ID before sign out)
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
        {
             await _systemLogService.LogAsync("Logout", "Đăng xuất hệ thống.", userId, "NguoiDung", userIdStr, HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> TestFail(string username)
    {
        var user = await _userRepo.GetByUsernameAsync(username);
        if (user == null) return Content($"User '{username}' not found");
        
        await _userRepo.UpdateFailedLoginAsync(user.MaNguoiDung);
        
        // Fetch again to verify
        var check = await _userRepo.GetByUsernameAsync(username);
        return Content($"User: {check.TenDangNhap}, SoLanSai: {check.SoLanSai}, Active: {check.TrangThaiHoatDong}");
    }
}
