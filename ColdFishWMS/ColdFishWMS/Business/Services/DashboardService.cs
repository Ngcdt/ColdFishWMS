using ColdFishWMS.Data;
using ColdFishWMS.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Business.Services;

public class DashboardService : IDashboardService
{
    private readonly ColdFishDbContext _context;

    public DashboardService(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardDTO> GetDashboardDataAsync(int days = 14)
    {
        var tongSanPham = await _context.SanPhams.CountAsync();
        var tongLoHang = await _context.LoHangs.CountAsync();
        var tongGiaTriTon = await _context.LoHangs
            .Join(_context.SanPhams, lh => lh.MaSanPham, sp => sp.MaSanPham,
                (lh, sp) => new { lh.SoLuongTon, sp.GiaNhapMacDinh })
            .SumAsync(x => x.SoLuongTon * x.GiaNhapMacDinh);

        var soLoSapHetHan = await _context.LoHangs.CountAsync(lh => lh.HanSuDung <= DateTime.Today.AddDays(30));
        var soCanhBaoChuaXuLy = await _context.CanhBaos.CountAsync(cb => !cb.DaXuLy);
        var canhBaoMoiNhat = await _context.CanhBaos
            .OrderByDescending(cb => cb.NgayTao)
            .Take(5)
            .ToListAsync();

        // 1. Thống kê nhập xuất hôm nay
        var today = DateTime.Today;
        var nhapHomNay = await _context.PhieuNhaps.CountAsync(x => x.NgayTao.Date == today);
        var xuatHomNay = await _context.PhieuXuats.CountAsync(x => x.NgayTao.Date == today);
        var soLuotNhapXuat = nhapHomNay + xuatHomNay;

        // 2. Hoạt động gần đây (Lấy 5 nhập, 5 xuất -> Merge -> Sort -> Take 5)
        var recentNhaps = await _context.PhieuNhaps
            .OrderByDescending(x => x.NgayTao)
            .Take(5)
            .Select(x => new HoatDongDTO 
            { 
                NoiDung = "Nhập kho " + x.MaPhieuNhap, 
                ThoiGian = x.NgayTao 
            })
            .ToListAsync();

        var recentXuats = await _context.PhieuXuats
            .OrderByDescending(x => x.NgayTao)
            .Take(5)
            .Select(x => new HoatDongDTO 
            { 
                NoiDung = "Xuất kho " + x.MaPhieuXuat, 
                ThoiGian = x.NgayTao 
            })
            .ToListAsync();

        var hoatDong = recentNhaps
            .Concat(recentXuats)
            .OrderByDescending(x => x.ThoiGian)
            .Take(5)
            .ToList();

        // 3. Biểu đồ dynamic days
        var daysWindow = days;
        var startChartDate = DateTime.Today.AddDays(-(daysWindow - 1));
        var chartLabels = new List<string>();
        var chartDataNhap = new List<decimal>();
        var chartDataXuat = new List<decimal>();
        var chartCountNhap = new List<int>();
        var chartCountXuat = new List<int>();
        decimal tongNhapKy = 0;
        decimal tongXuatKy = 0;

        for (var i = 0; i < daysWindow; i++)
        {
            var date = startChartDate.AddDays(i);
            chartLabels.Add(date.ToString("dd/MM"));
            
            var nhaps = await _context.PhieuNhaps.Where(x => x.NgayNhap.Date == date).ToListAsync();
            var xuats = await _context.PhieuXuats.Where(x => x.NgayXuat.Date == date).ToListAsync();

            var totalNhap = nhaps.Sum(x => x.TongTien);
            var countNhap = nhaps.Count;

            var totalXuat = xuats.Sum(x => x.TongTien);
            var countXuat = xuats.Count;
            
            chartDataNhap.Add(totalNhap);
            chartDataXuat.Add(totalXuat);
            chartCountNhap.Add(countNhap);
            chartCountXuat.Add(countXuat);
            
            tongNhapKy += totalNhap;
            tongXuatKy += totalXuat;
        }

        return new DashboardDTO
        {
            TongSanPham = tongSanPham,
            TongLoHang = tongLoHang,
            TongGiaTriTonKho = tongGiaTriTon,
            SoLoSapHetHan = soLoSapHetHan,
            SoCanhBaoChuaXuLy = soCanhBaoChuaXuLy,
            CanhBaoMoiNhat = canhBaoMoiNhat,
            SoLuotNhapXuatHomNay = soLuotNhapXuat,
            HoatDongGanDay = hoatDong,
            ChartLabels = chartLabels,
            ChartDataNhap = chartDataNhap,
            ChartDataXuat = chartDataXuat,
            ChartCountNhap = chartCountNhap,
            ChartCountXuat = chartCountXuat,
            TongNhapTrongKy = tongNhapKy,
            TongXuatTrongKy = tongXuatKy
        };
    }

    public async Task KiemTraVaTaoCanhBaoAsync()
    {
        var sapHetHan = await _context.LoHangs
            .Where(lh => lh.HanSuDung <= DateTime.Today.AddDays(7) && lh.SoLuongTon > 0)
            .ToListAsync();

        foreach (var lo in sapHetHan)
        {
            var exists = await _context.CanhBaos.AnyAsync(cb =>
                cb.MaLoHang == lo.MaLoHang && cb.LoaiCanhBao == "HSD" && !cb.DaXuLy);

            if (!exists)
            {
                _context.CanhBaos.Add(new Models.Entities.CanhBao
                {
                    LoaiCanhBao = "HSD",
                    NoiDung = $"Lô {lo.MaLoHang} sắp hết hạn",
                    MaSanPham = lo.MaSanPham,
                    MaLoHang = lo.MaLoHang,
                    MucDo = "Trung bình"
                });
            }
        }

        await _context.SaveChangesAsync();
    }
}

