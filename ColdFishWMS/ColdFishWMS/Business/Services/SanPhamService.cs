using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public class SanPhamService : ISanPhamService
{
    private readonly ISanPhamRepository _sanPhamRepository;

    public SanPhamService(ISanPhamRepository sanPhamRepository)
    {
        _sanPhamRepository = sanPhamRepository;
    }

    public Task<IEnumerable<SanPham>> GetAllAsync() => _sanPhamRepository.GetAllWithDetailsAsync();

    public Task<SanPham?> GetByIdAsync(string id) => _sanPhamRepository.GetByIdAsync(id);

    public Task<SanPham> CreateAsync(SanPham sanPham)
    {
        sanPham.NgayTao = DateTime.Now;
        sanPham.TrangThaiHoatDong = true;
        return _sanPhamRepository.AddAsync(sanPham);
    }

    public async Task<SanPham> UpdateAsync(SanPham sanPham)
    {
        await _sanPhamRepository.UpdateAsync(sanPham);
        return sanPham;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var entity = await _sanPhamRepository.GetByIdAsync(id);
        if (entity == null)
            return false;

        await _sanPhamRepository.DeleteAsync(entity);
        return true;
    }

    public async Task<IEnumerable<SanPham>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await _sanPhamRepository.GetAllAsync();

        return await _sanPhamRepository.FindAsync(s =>
            s.TenSanPham.Contains(keyword) || s.MaSanPham == keyword);
    }
}

