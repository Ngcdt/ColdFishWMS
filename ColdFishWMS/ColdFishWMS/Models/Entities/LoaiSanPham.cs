using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("LoaiSanPham")]
public class LoaiSanPham
{
    [Key]
    public int MaLoai { get; set; }

    [Required(ErrorMessage = "Tên loại không được để trống")]
    [MaxLength(100)]
    public string TenLoai { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTa { get; set; }
}
