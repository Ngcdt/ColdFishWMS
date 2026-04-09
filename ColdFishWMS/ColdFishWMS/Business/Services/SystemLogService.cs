using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Business.Services;

public class SystemLogService : ISystemLogService
{
    private readonly ColdFishDbContext _context;

    public SystemLogService(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string content, int? userId = null, string? entityType = null, string? entityId = null, string? ipAddress = null)
    {
        var log = new NhatKyHeThong
        {
            MaNguoiDung = userId,
            HanhDong = action,
            NoiDung = content,
            LoaiDoiTuong = entityType,
            MaDoiTuong = entityId,
            IPAddress = ipAddress,
            NgayTao = DateTime.Now
        };

        _context.NhatKyHeThongs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NhatKyHeThong>> GetLogsAsync(int page = 1, int pageSize = 50, string? search = null)
    {
        var query = _context.NhatKyHeThongs
            .Include(l => l.NguoiDung)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l => 
                l.NoiDung.Contains(search) || 
                l.HanhDong.Contains(search) ||
                (l.NguoiDung != null && l.NguoiDung.TenDangNhap.Contains(search)));
        }

        return await query.OrderByDescending(l => l.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
