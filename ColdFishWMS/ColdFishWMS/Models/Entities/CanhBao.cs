using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("CanhBao")]
public class CanhBao
{
    [Key]
    public int MaCanhBao { get; set; }

    [Required, MaxLength(100)]
    public string LoaiCanhBao { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string NoiDung { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MaSanPham { get; set; }

    [ForeignKey("MaSanPham")]
    public SanPham? SanPham { get; set; }

    [MaxLength(50)]
    public string? MaLoHang { get; set; }

    [ForeignKey("MaLoHang")]
    public LoHang? LoHang { get; set; }

    [MaxLength(20)]
    public string? MucDo { get; set; }

    public bool DaXuLy { get; set; } = false;
    public DateTime NgayTao { get; set; } = DateTime.Now;
    public DateTime? NgayXuLy { get; set; }
}

