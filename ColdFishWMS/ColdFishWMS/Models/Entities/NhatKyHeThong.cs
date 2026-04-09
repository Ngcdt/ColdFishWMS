using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("NhatKyHeThong")]
public class NhatKyHeThong
{
    [Key]
    public int Id { get; set; }

    public int? MaNguoiDung { get; set; }

    [ForeignKey("MaNguoiDung")]
    public virtual NguoiDung? NguoiDung { get; set; }

    [Required]
    [MaxLength(100)]
    public string HanhDong { get; set; } = string.Empty; // Login, Create, Update, Delete

    [Required]
    public string NoiDung { get; set; } = string.Empty; // Description of action

    [MaxLength(50)]
    public string? LoaiDoiTuong { get; set; } // SanPham, NguoiDung...

    [MaxLength(50)]
    public string? MaDoiTuong { get; set; } // ID of the object

    public DateTime NgayTao { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string? IPAddress { get; set; }
}
