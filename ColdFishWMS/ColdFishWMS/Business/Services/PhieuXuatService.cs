using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Data;
using ColdFishWMS.Data.Repositories;

namespace ColdFishWMS.Business.Services;

public class PhieuXuatService : IPhieuXuatService
{
    private readonly ColdFishDbContext _context;
    private readonly ILoHangRepository _loHangRepo;

    public PhieuXuatService(ColdFishDbContext context, ILoHangRepository loHangRepo)
    {
        _context = context;
        _loHangRepo = loHangRepo;
    }

    public async Task<IEnumerable<PhieuXuat>> GetAllAsync()
    {
        return await _context.PhieuXuats
            .Include(p => p.KhachHang)
            .Include(p => p.NguoiTao)
            .Include(p => p.ChiTietPhieuXuats)
                .ThenInclude(c => c.SanPham)
            .OrderByDescending(p => p.NgayXuat)
            .ToListAsync();
    }

    public async Task<PhieuXuat?> GetByIdAsync(string id)
    {
        return await _context.PhieuXuats
            .Include(p => p.KhachHang)
            .Include(p => p.NguoiTao)
            .Include(p => p.ChiTietPhieuXuats)
                .ThenInclude(c => c.SanPham)
            .Include(p => p.ChiTietPhieuXuats)
                .ThenInclude(c => c.LoHang)
            .FirstOrDefaultAsync(p => p.MaPhieuXuat == id);
    }

    public async Task<List<LoHang>> GetDeXuatFEFOAsync(string maSanPham, decimal soLuongCanXuat)
    {
        var loHangList = await _loHangRepo.GetLoHangTheoFEFOAsync(maSanPham);
        var deXuat = new List<LoHang>();
        decimal soLuongDaChon = 0;

        foreach (var loHang in loHangList)
        {
            if (soLuongDaChon >= soLuongCanXuat)
                break;

            deXuat.Add(loHang);
            soLuongDaChon += loHang.SoLuongTon;
        }

        return deXuat;
    }

    public async Task<PhieuXuat> CreateWithFEFOAsync(PhieuXuat phieuXuat, List<ColdFishWMS.Models.DTOs.PhieuXuatItemDto> danhSachXuat)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            phieuXuat.MaPhieuXuat = await TaoSoPhieuXuatAsync();
            phieuXuat.NgayTao = DateTime.Now;
            phieuXuat.DaXuat = false;

            _context.PhieuXuats.Add(phieuXuat);
            await _context.SaveChangesAsync();

            decimal tongTien = 0;

            foreach (var item in danhSachXuat)
            {
                var deXuatFEFO = await GetDeXuatFEFOAsync(item.MaSanPham, item.SoLuong);
                decimal soLuongConLai = item.SoLuong;
                
                // Keep trying to fill order
                // Warning: If stock is insufficient, it will partial fill. 
                // But we should use the item Price for calculation for ALL processed qty.

                foreach (var loHang in deXuatFEFO)
                {
                    if (soLuongConLai <= 0)
                        break;

                    decimal soLuongXuat = Math.Min(soLuongConLai, loHang.SoLuongTon);

                    var chiTiet = new ChiTietPhieuXuat
                    {
                        MaPhieuXuat = phieuXuat.MaPhieuXuat,
                        MaSanPham = item.MaSanPham,
                        MaLoHang = loHang.MaLoHang,
                        SoLuong = soLuongXuat,
                        DonGia = item.DonGia // Use provided price
                    };

                    _context.ChiTietPhieuXuats.Add(chiTiet);

                    tongTien += soLuongXuat * item.DonGia; // Add to total

                    loHang.SoLuongTon -= soLuongXuat;
                    soLuongConLai -= soLuongXuat;
                }
            }

            phieuXuat.TongTien = tongTien; // Update Total

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return phieuXuat;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<string> TaoSoPhieuXuatAsync()
    {
        var count = await _context.PhieuXuats.CountAsync();
        return $"PX{DateTime.Now:yyyyMMdd}{(count + 1):D4}";
    }
}

