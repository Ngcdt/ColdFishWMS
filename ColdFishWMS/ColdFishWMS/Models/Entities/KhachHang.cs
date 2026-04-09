using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("KhachHang")]
public class KhachHang
{
    [Key]
    [MaxLength(50)]
    public string MaKhachHang { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TenKhachHang { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? DiaChi { get; set; }

    [MaxLength(20)]
    public string? SoDienThoai { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;

    public ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
}





