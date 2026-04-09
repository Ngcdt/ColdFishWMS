using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("VaiTro")]
public class VaiTro
{
    [Key]
    public int MaVaiTro { get; set; }

    [Required, MaxLength(100)]
    public string TenVaiTro { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? MoTa { get; set; }

    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}

