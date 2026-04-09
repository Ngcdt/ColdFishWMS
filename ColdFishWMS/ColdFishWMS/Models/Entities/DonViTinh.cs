using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdFishWMS.Models.Entities;

[Table("DonViTinh")]
public class DonViTinh
{
    [Key]
    public int MaDonViTinh { get; set; }

    [Required, MaxLength(50)]
    public string TenDonViTinh { get; set; } = string.Empty;

    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}





