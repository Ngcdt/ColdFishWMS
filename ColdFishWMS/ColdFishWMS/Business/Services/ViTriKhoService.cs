using ColdFishWMS.Data.Repositories;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public class ViTriKhoService : IViTriKhoService
{
    private readonly IViTriKhoRepository _repository;

    public ViTriKhoService(IViTriKhoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ViTriKho>> GetAllAsync()
    {
        return await _repository.GetAllWithDetailsAsync();
    }

    public async Task<ViTriKho?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddAsync(ViTriKho entity)
    {
        // Auto-generate Code if empty? For now assume manual or simple logic
        // But Controller might handle validation
        await _repository.AddAsync(entity);
    }

    public async Task UpdateAsync(ViTriKho entity)
    {
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity != null)
        {
            await _repository.DeleteAsync(entity);
        }
    }
}
