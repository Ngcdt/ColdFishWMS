using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Models.ViewModels;

public class UserManagementViewModel
{
    // Stats
    public int TongNguoiDung { get; set; }
    public int NguoiDungHoatDong { get; set; }
    public int SoLuongVaiTro { get; set; }
    public int DangNhapHomNay { get; set; }

    // List
    public IEnumerable<NguoiDung> DanhSachNguoiDung { get; set; } = new List<NguoiDung>();
    
    // Filters (For UI state if needed, but logic usually in Controller)
    public string TuKhoa { get; set; } = "";
    public int? MaVaiTroFilter { get; set; }
    public bool? TrangThaiFilter { get; set; }
}
