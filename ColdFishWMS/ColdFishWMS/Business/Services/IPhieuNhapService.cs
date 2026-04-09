using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface IPhieuNhapService
{
    Task<IEnumerable<PhieuNhap>> GetAllAsync();
    Task<PhieuNhap?> GetByIdAsync(string id);
    Task<PhieuNhap> CreateAsync(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> chiTietList);
    Task<bool> DeleteAsync(string id);
    Task<bool> DuyetPhieuNhapAsync(string id);
}

