using ColdFishWMS.Models.DTOs;

namespace ColdFishWMS.Business.Services;

public interface IDashboardService
{
    Task<DashboardDTO> GetDashboardDataAsync(int days = 14);
    Task KiemTraVaTaoCanhBaoAsync();
}

