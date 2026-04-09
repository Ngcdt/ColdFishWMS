using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Data.Repositories;

public interface IViTriKhoRepository : IRepository<ViTriKho>
{
    Task<IEnumerable<ViTriKho>> GetAllWithDetailsAsync();
}
