using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface ISanPhamService
{
    Task<IEnumerable<SanPham>> GetAllAsync();
    Task<SanPham?> GetByIdAsync(string id);
    Task<SanPham> CreateAsync(SanPham sanPham);
    Task<SanPham> UpdateAsync(SanPham sanPham);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<SanPham>> SearchAsync(string keyword);
}

