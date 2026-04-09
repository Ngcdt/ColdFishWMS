using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("PhieuXuat")]
public class PhieuXuat
{
    [Key]
    [MaxLength(50)]
    public string MaPhieuXuat { get; set; } = string.Empty;

    [NotMapped]
    public string Id { get => MaPhieuXuat; set => MaPhieuXuat = value; }

    public DateTime NgayXuat { get; set; } = DateTime.Now;

    [MaxLength(50)]
    public string MaKhachHang { get; set; } = string.Empty;

    [ForeignKey("MaKhachHang")]
    public KhachHang? KhachHang { get; set; }

    public int MaNguoiTao { get; set; }

    [ForeignKey("MaNguoiTao")]
    public NguoiDung? NguoiTao { get; set; }

    public decimal TongTien { get; set; }

    [MaxLength(1000)]
    public string? GhiChu { get; set; }

    public bool DaXuat { get; set; } = false;
    public DateTime NgayTao { get; set; } = DateTime.Now;

    public ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();

    [NotMapped]
    public int LoHangId { get; set; }

    [NotMapped]
    public int SoLuong { get; set; }
}

