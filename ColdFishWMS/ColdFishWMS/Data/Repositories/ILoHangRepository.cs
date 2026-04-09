using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Data.Repositories;

public interface ILoHangRepository : IRepository<LoHang>
{
    Task<IEnumerable<LoHang>> GetWithSanPhamAsync();
    Task<List<LoHang>> GetLoHangTheoFEFOAsync(string maSanPham);
}

