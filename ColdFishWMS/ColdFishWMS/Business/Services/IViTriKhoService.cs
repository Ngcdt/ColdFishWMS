using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface IViTriKhoService
{
    Task<IEnumerable<ViTriKho>> GetAllAsync();
    Task<ViTriKho?> GetByIdAsync(string id);
    Task AddAsync(ViTriKho entity);
    Task UpdateAsync(ViTriKho entity);
    Task DeleteAsync(string id);
}
