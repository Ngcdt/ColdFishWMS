using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("ChiTietPhieuXuat")]
public class ChiTietPhieuXuat
{
    [Key]
    public int MaChiTiet { get; set; }

    [MaxLength(50)]
    public string MaPhieuXuat { get; set; } = string.Empty;

    [ForeignKey("MaPhieuXuat")]
    public PhieuXuat? PhieuXuat { get; set; }

    [MaxLength(50)]
    public string MaSanPham { get; set; } = string.Empty;

    [ForeignKey("MaSanPham")]
    public SanPham? SanPham { get; set; }

    [MaxLength(50)]
    public string MaLoHang { get; set; } = string.Empty;

    [ForeignKey("MaLoHang")]
    public LoHang? LoHang { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }

    [NotMapped]
    public decimal ThanhTien => SoLuong * DonGia;
}





