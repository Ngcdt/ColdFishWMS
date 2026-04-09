using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.DTOs;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public class AuthService : IAuthService
{
    private readonly INguoiDungRepository _nguoiDungRepo;

    public AuthService(INguoiDungRepository repo)
    {
        _nguoiDungRepo = repo;
    }

    public async Task<NguoiDung?> LoginAsync(LoginDTO model)
    {
        // 1. Tìm user theo username
        var user = await _nguoiDungRepo.GetByUsernameAsync(model.TenDangNhap?.Trim() ?? "");

        // 2. Kiểm tra tồn tại
        if (user == null)
            return null;

        // 3. Kiểm tra trạng thái hoạt động (TrangThaiHoatDong is bool)
        // Lưu ý: Nếu user.TrangThaiHoatDong là false -> return null
        if (!user.TrangThaiHoatDong)
            return null;

        // 4. Kiểm tra mật khẩu (Sử dụng BCrypt)
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.MatKhau, user.MatKhau);
        if (!isPasswordValid)
        {
            await _nguoiDungRepo.UpdateFailedLoginAsync(user.MaNguoiDung);
            return null;
        }

        // 5. Reset số lần sai nếu đăng nhập thành công
        await _nguoiDungRepo.ResetFailedLoginAsync(user.MaNguoiDung);

        return user;
    }
}
