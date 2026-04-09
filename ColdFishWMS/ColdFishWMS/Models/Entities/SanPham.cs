using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("SanPham")]
public class SanPham
{
    [Key]
    [Required, MaxLength(50)]
    public string MaSanPham { get; set; } = string.Empty;

    [NotMapped]
    public string Id { get => MaSanPham; set => MaSanPham = value; }

    [Required, MaxLength(200)]
    public string TenSanPham { get; set; } = string.Empty;

    [NotMapped]
    public string Ten { get => TenSanPham; set => TenSanPham = value; }

    [MaxLength(1000)]
    public string? MoTa { get; set; }

    public int DinhMucTonThap { get; set; } = 10;
    public double NhietDoToiDa { get; set; } = 5.0;
    public double NhietDoToiThieu { get; set; } = -18.0;

    public int MaDonViTinh { get; set; }

    [ForeignKey("MaDonViTinh")]
    public DonViTinh? DonViTinh { get; set; }

    [MaxLength(50)]
    public string? MaNhaCungCap { get; set; }
    [ForeignKey("MaNhaCungCap")]
    public NhaCungCap? NhaCungCap { get; set; }

    public int? MaLoai { get; set; }
    [ForeignKey("MaLoai")]
    public LoaiSanPham? LoaiSanPham { get; set; }

    [NotMapped]
    public string DonViTinhText { get => DonViTinh?.TenDonViTinh ?? string.Empty; set { } }

    public decimal GiaNhapMacDinh { get; set; }

    public bool TrangThaiHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
    public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
    public ICollection<LoHang> LoHangs { get; set; } = new List<LoHang>();

    [NotMapped]
    public int SoLuongTon { get; set; }
}

