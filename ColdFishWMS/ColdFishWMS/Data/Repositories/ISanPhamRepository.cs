using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Data.Repositories;

public interface ISanPhamRepository : IRepository<SanPham>
{
    Task<IEnumerable<SanPham>> GetWithLoHangsAsync();
    Task<IEnumerable<SanPham>> GetAllWithDetailsAsync();
}





