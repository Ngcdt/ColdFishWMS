using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Data.Repositories;

public class SanPhamRepository : Repository<SanPham>, ISanPhamRepository
{
    private readonly ColdFishDbContext _context;

    public SanPhamRepository(ColdFishDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SanPham>> GetWithLoHangsAsync()
    {
        return await _context.SanPhams.Include(x => x.LoHangs).ToListAsync();
    }

    public async Task<IEnumerable<SanPham>> GetAllWithDetailsAsync()
    {
        return await _context.SanPhams
            .Include(x => x.DonViTinh)
            .Include(x => x.NhaCungCap)
            .Include(x => x.LoaiSanPham)
            .OrderByDescending(x => x.MaSanPham)
            .ToListAsync();
    }
}





