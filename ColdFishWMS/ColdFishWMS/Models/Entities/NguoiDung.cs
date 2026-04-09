using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("NguoiDung")]
public class NguoiDung
{
    [Key]
    public int MaNguoiDung { get; set; }

    [NotMapped]
    public int Id { get => MaNguoiDung; set => MaNguoiDung = value; }

    [Required, MaxLength(100)]
    public string TenDangNhap { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string MatKhau { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string HoTen { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    public int MaVaiTro { get; set; }

    [ForeignKey("MaVaiTro")]
    public VaiTro? VaiTro { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayCapNhat { get; set; }
    public int SoLanSai { get; set; } = 0;
    public string? TrangThai { get; set; }

}

