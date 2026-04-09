using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Data;

namespace ColdFishWMS.Business.Services;

public class PhieuNhapService : IPhieuNhapService
{
    private readonly ColdFishDbContext _context;

    public PhieuNhapService(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PhieuNhap>> GetAllAsync()
    {
        return await _context.PhieuNhaps
            .Include(p => p.NhaCungCap)
            .Include(p => p.NguoiTao)
            .Include(p => p.ChiTietPhieuNhaps)
                .ThenInclude(c => c.SanPham)
            .OrderByDescending(p => p.NgayNhap)
            .ToListAsync();
    }

    public async Task<PhieuNhap?> GetByIdAsync(string id)
    {
        return await _context.PhieuNhaps
            .Include(p => p.NhaCungCap)
            .Include(p => p.NguoiTao)
            .Include(p => p.ChiTietPhieuNhaps)
                .ThenInclude(c => c.SanPham)
            .Include(p => p.ChiTietPhieuNhaps)
                .ThenInclude(c => c.LoHang)
            .FirstOrDefaultAsync(p => p.MaPhieuNhap == id);
    }

    public async Task<PhieuNhap> CreateAsync(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> chiTietList)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            phieuNhap.MaPhieuNhap = await TaoSoPhieuNhapAsync();
            phieuNhap.NgayTao = DateTime.Now;
            phieuNhap.DaDuyet = false;

            _context.PhieuNhaps.Add(phieuNhap);
            await _context.SaveChangesAsync();

            decimal tongTien = 0;

            foreach (var chiTiet in chiTietList)
            {
                // New logic: Defer Lot creation. MaLoHang/LoHang will be null.
                chiTiet.MaPhieuNhap = phieuNhap.MaPhieuNhap;
                
                // Only create LoHang if provided (legacy support or rigorous check)
                // But generally we expect null here now.
                // existing code was forced.

                _context.ChiTietPhieuNhaps.Add(chiTiet);
                
                tongTien += chiTiet.SoLuong * chiTiet.DonGia; // Add to total
            }

            phieuNhap.TongTien = tongTien; // Update Total Amount

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return phieuNhap;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var phieuNhap = await GetByIdAsync(id);
        if (phieuNhap == null || phieuNhap.DaDuyet)
            return false;

        _context.PhieuNhaps.Remove(phieuNhap);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DuyetPhieuNhapAsync(string id)
    {
        var phieuNhap = await GetByIdAsync(id);
        if (phieuNhap == null)
            return false;

        phieuNhap.DaDuyet = true;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<string> TaoSoPhieuNhapAsync()
    {
        var count = await _context.PhieuNhaps.CountAsync();
        return $"PN{DateTime.Now:yyyyMMdd}{(count + 1):D4}";
    }
}

