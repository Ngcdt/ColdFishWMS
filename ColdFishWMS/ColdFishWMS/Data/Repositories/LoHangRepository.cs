using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Data.Repositories;

public class LoHangRepository : Repository<LoHang>, ILoHangRepository
{
    private readonly ColdFishDbContext _context;

    public LoHangRepository(ColdFishDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LoHang>> GetWithSanPhamAsync()
    {
        return await _context.LoHangs.Include(x => x.SanPham).ToListAsync();
    }

    public async Task<List<LoHang>> GetLoHangTheoFEFOAsync(string maSanPham)
    {
        return await _context.LoHangs
            .Include(l => l.SanPham)
            .Include(l => l.ViTriKho)
            .Where(l => l.MaSanPham == maSanPham && l.SoLuongTon > 0)
            .OrderBy(l => l.HanSuDung)
            .ToListAsync();
    }
}

