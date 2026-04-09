using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("ViTriKho")]
public class ViTriKho
{
    [Key]
    [Required, MaxLength(50)]
    public string MaViTri { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TenViTri { get; set; }

    [MaxLength(50)]
    public string? Khu { get; set; }

    [MaxLength(50)]
    public string? Ke { get; set; }

    [MaxLength(50)]
    public string? Tang { get; set; }

    public bool TrangThaiTrong { get; set; } = true;

    public ICollection<LoHang> LoHangs { get; set; } = new List<LoHang>();
}





