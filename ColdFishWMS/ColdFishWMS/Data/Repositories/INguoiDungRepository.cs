using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Data.Repositories;

public interface INguoiDungRepository
{
    // Tìm user theo username
    Task<NguoiDung?> GetByUsernameAsync(string username);

    // Cập nhật số lần đăng nhập sai
    Task UpdateFailedLoginAsync(int userId);

    // Reset số lần sai khi đăng nhập thành công
    Task ResetFailedLoginAsync(int userId);
}
