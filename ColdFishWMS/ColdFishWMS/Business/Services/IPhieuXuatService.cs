using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface IPhieuXuatService
{
    Task<IEnumerable<PhieuXuat>> GetAllAsync();
    Task<PhieuXuat?> GetByIdAsync(string id);
    Task<PhieuXuat> CreateWithFEFOAsync(PhieuXuat phieuXuat, List<Models.DTOs.PhieuXuatItemDto> danhSachXuat);
    Task<List<LoHang>> GetDeXuatFEFOAsync(string maSanPham, decimal soLuongCanXuat);
}

