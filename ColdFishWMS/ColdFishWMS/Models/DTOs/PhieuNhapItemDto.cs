namespace ColdFishWMS.Models.DTOs;

public class PhieuNhapItemDto
{
    public string MaSanPham { get; set; }
    public string MaLoHang { get; set; }
    public DateTime NgaySanXuat { get; set; }
    public DateTime HanSuDung { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string MaViTri { get; set; }
}
