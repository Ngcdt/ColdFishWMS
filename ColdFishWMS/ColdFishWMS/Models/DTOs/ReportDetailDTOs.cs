namespace ColdFishWMS.Models.DTOs;

public class ReportImportDetailDTO
{
    public string MaPhieu { get; set; }
    public DateTime NgayNhap { get; set; }
    public string NhaCungCap { get; set; }
    public string MaSanPham { get; set; }
    public string TenSanPham { get; set; }
    public string MaLoHang { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class ReportExportDetailDTO
{
    public string MaPhieu { get; set; }
    public DateTime NgayXuat { get; set; }
    public string KhachHang { get; set; }
    public string MaSanPham { get; set; }
    public string TenSanPham { get; set; }
    public string MaLoHang { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class ReportInventoryDetailDTO
{
    public string MaLo { get; set; }
    public string MaSanPham { get; set; }
    public string TenSanPham { get; set; }
    public string ViTri { get; set; }
    public DateTime NgaySanXuat { get; set; }
    public DateTime NgayNhap { get; set; } // Added for Report Header Date Range
    public DateTime HanSuDung { get; set; }
    public decimal SoLuongTon { get; set; }
    public string TrangThai { get; set; }
}
