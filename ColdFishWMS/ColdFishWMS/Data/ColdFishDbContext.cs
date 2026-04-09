using ColdFishWMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Data;

public class ColdFishDbContext : DbContext
{
    public ColdFishDbContext(DbContextOptions<ColdFishDbContext> options) : base(options)
    {
    }

    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<LoaiSanPham> LoaiSanPhams => Set<LoaiSanPham>();
    public DbSet<DonViTinh> DonViTinhs => Set<DonViTinh>();
    public DbSet<NhaCungCap> NhaCungCaps => Set<NhaCungCap>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<ViTriKho> ViTriKhos => Set<ViTriKho>();
    public DbSet<LoHang> LoHangs => Set<LoHang>();
    public DbSet<PhieuNhap> PhieuNhaps => Set<PhieuNhap>();
    public DbSet<PhieuXuat> PhieuXuats => Set<PhieuXuat>();
    public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps => Set<ChiTietPhieuNhap>();
    public DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats => Set<ChiTietPhieuXuat>();
    public DbSet<CanhBao> CanhBaos => Set<CanhBao>();
    public DbSet<NhatKyHeThong> NhatKyHeThongs => Set<NhatKyHeThong>();
    public DbSet<NhatKyNhietDo> NhatKyNhietDos => Set<NhatKyNhietDo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NguoiDung>()
            .HasIndex(x => x.TenDangNhap)
            .IsUnique();

        // Tránh multiple cascade paths
        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasOne(ct => ct.SanPham)
            .WithMany(sp => sp.ChiTietPhieuNhaps)
            .HasForeignKey(ct => ct.MaSanPham)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasOne(ct => ct.LoHang)
            .WithMany(lh => lh.ChiTietPhieuNhaps)
            .HasForeignKey(ct => ct.MaLoHang)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChiTietPhieuXuat>()
            .HasOne(ct => ct.SanPham)
            .WithMany(sp => sp.ChiTietPhieuXuats)
            .HasForeignKey(ct => ct.MaSanPham)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChiTietPhieuXuat>()
            .HasOne(ct => ct.LoHang)
            .WithMany(lh => lh.ChiTietPhieuXuats)
            .HasForeignKey(ct => ct.MaLoHang)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

