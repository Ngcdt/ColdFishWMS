using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("PhieuNhap")]
public class PhieuNhap
{
    [Key]
    [MaxLength(50)]
    public string MaPhieuNhap { get; set; } = string.Empty;

    [NotMapped]
    public string Id { get => MaPhieuNhap; set => MaPhieuNhap = value; }

    public DateTime NgayNhap { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string MaNhaCungCap { get; set; } = string.Empty;

    [ForeignKey("MaNhaCungCap")]
    public NhaCungCap? NhaCungCap { get; set; }

    public int MaNguoiTao { get; set; }

    [ForeignKey("MaNguoiTao")]
    public NguoiDung? NguoiTao { get; set; }

    public decimal TongTien { get; set; }

    [MaxLength(1000)]
    public string? GhiChu { get; set; }

    public bool DaDuyet { get; set; } = false;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();

    [NotMapped]
    public int LoHangId { get; set; }

    [NotMapped]
    public int SoLuong { get; set; }
}

