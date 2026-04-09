using System.ComponentModel.DataAnnotations;

namespace ColdFishWMS.Models.DTOs;

public class LoginDTO
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [DataType(DataType.Password)]
    public string MatKhau { get; set; } = string.Empty;

    public bool GhiNhoDangNhap { get; set; }
}

