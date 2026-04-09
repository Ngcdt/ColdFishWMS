using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("ChiTietPhieuNhap")]
public class ChiTietPhieuNhap
{
    [Key]
    public int MaChiTiet { get; set; }

    [MaxLength(50)]
    public string MaPhieuNhap { get; set; } = string.Empty;

    [ForeignKey("MaPhieuNhap")]
    public PhieuNhap? PhieuNhap { get; set; }

    [MaxLength(50)]
    public string MaSanPham { get; set; } = string.Empty;

    [ForeignKey("MaSanPham")]
    public SanPham? SanPham { get; set; }

    [MaxLength(50)]
    public string? MaLoHang { get; set; }

    [ForeignKey("MaLoHang")]
    public LoHang? LoHang { get; set; }

    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }

    [NotMapped]
    public decimal ThanhTien => SoLuong * DonGia;
}





