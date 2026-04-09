using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Data.Repositories;

public class ViTriKhoRepository : Repository<ViTriKho>, IViTriKhoRepository
{
    private ColdFishDbContext _context => Context as ColdFishDbContext;

    public ViTriKhoRepository(ColdFishDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ViTriKho>> GetAllWithDetailsAsync()
    {
        // Include LoHangs to get batch info as requested
        return await _context.ViTriKhos
            .Include(x => x.LoHangs)
            .OrderBy(x => x.MaViTri)
            .ToListAsync();
    }
}
