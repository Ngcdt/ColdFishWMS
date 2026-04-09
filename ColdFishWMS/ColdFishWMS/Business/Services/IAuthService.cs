using ColdFishWMS.Models.DTOs;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Business.Services;

public interface IAuthService
{
    Task<NguoiDung?> LoginAsync(LoginDTO model);
}
