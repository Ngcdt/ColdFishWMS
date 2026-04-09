using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface ISystemLogService
{
    Task LogAsync(string action, string content, int? userId = null, string? entityType = null, string? entityId = null, string? ipAddress = null);
    Task<List<NhatKyHeThong>> GetLogsAsync(int page = 1, int pageSize = 50, string? search = null);
}
