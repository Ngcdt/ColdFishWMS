namespace ColdFishWMS.Models.DTOs;

public class DashboardDTO
{
    public int TongSanPham { get; set; }
    public int TongLoHang { get; set; }
    public decimal TongGiaTriTonKho { get; set; }
    public int SoLoSapHetHan { get; set; }
    public int SoCanhBaoChuaXuLy { get; set; }
    public List<ColdFishWMS.Models.Entities.CanhBao> CanhBaoMoiNhat { get; set; } = new();
    
    // New fields
    public int SoLuotNhapXuatHomNay { get; set; }
    public List<HoatDongDTO> HoatDongGanDay { get; set; } = new();

    // Chart Data
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> ChartDataNhap { get; set; } = new();
    public List<decimal> ChartDataXuat { get; set; } = new();
    public List<int> ChartCountNhap { get; set; } = new();
    public List<int> ChartCountXuat { get; set; } = new();
    
    // Totals for Chart
    public decimal TongNhapTrongKy { get; set; }
    public decimal TongXuatTrongKy { get; set; }
}

public class HoatDongDTO
{
    public string NoiDung { get; set; } = string.Empty;
    public DateTime ThoiGian { get; set; }
}

