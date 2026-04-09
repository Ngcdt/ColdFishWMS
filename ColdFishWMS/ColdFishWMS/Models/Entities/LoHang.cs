using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("LoHang")]
public class LoHang
{
    [Key]
    [MaxLength(50)]
    public string MaLoHang { get; set; } = string.Empty;

    [NotMapped]
    public string Id { get => MaLoHang; set => MaLoHang = value; }

    [MaxLength(50)]
    public string MaSanPham { get; set; } = string.Empty;

    [ForeignKey("MaSanPham")]
    public SanPham? SanPham { get; set; }

    public DateTime NgaySanXuat { get; set; }
    public DateTime HanSuDung { get; set; }

    public decimal SoLuongNhap { get; set; }
    public decimal SoLuongTon { get; set; }

    [MaxLength(50)]
    public string? MaViTri { get; set; }

    [ForeignKey("MaViTri")]
    public ViTriKho? ViTriKho { get; set; }

    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
    public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
}

