using ColdFishWMS.Data;
using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class NguoiDungRepository : INguoiDungRepository
{
    private readonly ColdFishDbContext _context;

    public NguoiDungRepository(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<NguoiDung?> GetByUsernameAsync(string username)
    {
        return await _context.NguoiDungs
            .Include(x => x.VaiTro)       // join VaiTro
            .FirstOrDefaultAsync(x => x.TenDangNhap == username);
    }

    public async Task UpdateFailedLoginAsync(int userId)
    {
        var user = await _context.NguoiDungs.FindAsync(userId);
        if (user != null)
        {
            user.SoLanSai += 1;
            if (user.SoLanSai >= 5)
            {
                user.TrangThai = "Ngưng";
                user.TrangThaiHoatDong = false; // Khóa tài khoản
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task ResetFailedLoginAsync(int userId)
    {
        var user = await _context.NguoiDungs.FindAsync(userId);
        if (user != null)
        {
            user.SoLanSai = 0;

            await _context.SaveChangesAsync();
        }
    }
}
