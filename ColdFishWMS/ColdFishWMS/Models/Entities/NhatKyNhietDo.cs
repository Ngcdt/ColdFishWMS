using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

public class NhatKyNhietDo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public double NhietDo { get; set; }

    public double? DoAm { get; set; }

    public DateTime ThoiGianGhi { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string MaThietBi { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MaViTri { get; set; }

    [ForeignKey("MaViTri")]
    public virtual ViTriKho? ViTriKho { get; set; }
}
