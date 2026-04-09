using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColdFishWMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonViTinh",
                columns: table => new
                {
                    MaDonViTinh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDonViTinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonViTinh", x => x.MaDonViTinh);
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "LoaiSanPham",
                columns: table => new
                {
                    MaLoai = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSanPham", x => x.MaLoai);
                });

            migrationBuilder.CreateTable(
                name: "NhaCungCap",
                columns: table => new
                {
                    MaNhaCungCap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenNhaCungCap = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhaCungCap", x => x.MaNhaCungCap);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "ViTriKho",
                columns: table => new
                {
                    MaViTri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenViTri = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Khu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Ke = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Tang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThaiTrong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViTriKho", x => x.MaViTri);
                });

            migrationBuilder.CreateTable(
                name: "SanPham",
                columns: table => new
                {
                    MaSanPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenSanPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DinhMucTonThap = table.Column<int>(type: "int", nullable: false),
                    NhietDoToiDa = table.Column<double>(type: "float", nullable: false),
                    NhietDoToiThieu = table.Column<double>(type: "float", nullable: false),
                    MaDonViTinh = table.Column<int>(type: "int", nullable: false),
                    MaNhaCungCap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaLoai = table.Column<int>(type: "int", nullable: true),
                    GiaNhapMacDinh = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPham", x => x.MaSanPham);
                    table.ForeignKey(
                        name: "FK_SanPham_DonViTinh_MaDonViTinh",
                        column: x => x.MaDonViTinh,
                        principalTable: "DonViTinh",
                        principalColumn: "MaDonViTinh",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SanPham_LoaiSanPham_MaLoai",
                        column: x => x.MaLoai,
                        principalTable: "LoaiSanPham",
                        principalColumn: "MaLoai");
                    table.ForeignKey(
                        name: "FK_SanPham_NhaCungCap_MaNhaCungCap",
                        column: x => x.MaNhaCungCap,
                        principalTable: "NhaCungCap",
                        principalColumn: "MaNhaCungCap");
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoLanSai = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoHang",
                columns: table => new
                {
                    MaLoHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgaySanXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanSuDung = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongNhap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongTon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaViTri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoHang", x => x.MaLoHang);
                    table.ForeignKey(
                        name: "FK_LoHang_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoHang_ViTriKho_MaViTri",
                        column: x => x.MaViTri,
                        principalTable: "ViTriKho",
                        principalColumn: "MaViTri");
                });

            migrationBuilder.CreateTable(
                name: "PhieuNhap",
                columns: table => new
                {
                    MaPhieuNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNhaCungCap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DaDuyet = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuNhap", x => x.MaPhieuNhap);
                    table.ForeignKey(
                        name: "FK_PhieuNhap_NguoiDung_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuNhap_NhaCungCap_MaNhaCungCap",
                        column: x => x.MaNhaCungCap,
                        principalTable: "NhaCungCap",
                        principalColumn: "MaNhaCungCap",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuXuat",
                columns: table => new
                {
                    MaPhieuXuat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaNguoiTao = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DaXuat = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuXuat", x => x.MaPhieuXuat);
                    table.ForeignKey(
                        name: "FK_PhieuXuat_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuXuat_NguoiDung_MaNguoiTao",
                        column: x => x.MaNguoiTao,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CanhBao",
                columns: table => new
                {
                    MaCanhBao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoaiCanhBao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaLoHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MucDo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayXuLy = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanhBao", x => x.MaCanhBao);
                    table.ForeignKey(
                        name: "FK_CanhBao_LoHang_MaLoHang",
                        column: x => x.MaLoHang,
                        principalTable: "LoHang",
                        principalColumn: "MaLoHang");
                    table.ForeignKey(
                        name: "FK_CanhBao_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietPhieuNhap",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietPhieuNhap", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuNhap_LoHang_MaLoHang",
                        column: x => x.MaLoHang,
                        principalTable: "LoHang",
                        principalColumn: "MaLoHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuNhap_PhieuNhap_MaPhieuNhap",
                        column: x => x.MaPhieuNhap,
                        principalTable: "PhieuNhap",
                        principalColumn: "MaPhieuNhap",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuNhap_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietPhieuXuat",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieuXuat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaSanPham = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoHang = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoLuong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietPhieuXuat", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuXuat_LoHang_MaLoHang",
                        column: x => x.MaLoHang,
                        principalTable: "LoHang",
                        principalColumn: "MaLoHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuXuat_PhieuXuat_MaPhieuXuat",
                        column: x => x.MaPhieuXuat,
                        principalTable: "PhieuXuat",
                        principalColumn: "MaPhieuXuat",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietPhieuXuat_SanPham_MaSanPham",
                        column: x => x.MaSanPham,
                        principalTable: "SanPham",
                        principalColumn: "MaSanPham",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanhBao_MaLoHang",
                table: "CanhBao",
                column: "MaLoHang");

            migrationBuilder.CreateIndex(
                name: "IX_CanhBao_MaSanPham",
                table: "CanhBao",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuNhap_MaLoHang",
                table: "ChiTietPhieuNhap",
                column: "MaLoHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuNhap_MaPhieuNhap",
                table: "ChiTietPhieuNhap",
                column: "MaPhieuNhap");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuNhap_MaSanPham",
                table: "ChiTietPhieuNhap",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuXuat_MaLoHang",
                table: "ChiTietPhieuXuat",
                column: "MaLoHang");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuXuat_MaPhieuXuat",
                table: "ChiTietPhieuXuat",
                column: "MaPhieuXuat");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietPhieuXuat_MaSanPham",
                table: "ChiTietPhieuXuat",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_LoHang_MaSanPham",
                table: "LoHang",
                column: "MaSanPham");

            migrationBuilder.CreateIndex(
                name: "IX_LoHang_MaViTri",
                table: "LoHang",
                column: "MaViTri");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaVaiTro",
                table: "NguoiDung",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_TenDangNhap",
                table: "NguoiDung",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhap_MaNguoiTao",
                table: "PhieuNhap",
                column: "MaNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuNhap_MaNhaCungCap",
                table: "PhieuNhap",
                column: "MaNhaCungCap");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuat_MaKhachHang",
                table: "PhieuXuat",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuXuat_MaNguoiTao",
                table: "PhieuXuat",
                column: "MaNguoiTao");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaDonViTinh",
                table: "SanPham",
                column: "MaDonViTinh");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaLoai",
                table: "SanPham",
                column: "MaLoai");

            migrationBuilder.CreateIndex(
                name: "IX_SanPham_MaNhaCungCap",
                table: "SanPham",
                column: "MaNhaCungCap");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanhBao");

            migrationBuilder.DropTable(
                name: "ChiTietPhieuNhap");

            migrationBuilder.DropTable(
                name: "ChiTietPhieuXuat");

            migrationBuilder.DropTable(
                name: "PhieuNhap");

            migrationBuilder.DropTable(
                name: "LoHang");

            migrationBuilder.DropTable(
                name: "PhieuXuat");

            migrationBuilder.DropTable(
                name: "SanPham");

            migrationBuilder.DropTable(
                name: "ViTriKho");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "DonViTinh");

            migrationBuilder.DropTable(
                name: "LoaiSanPham");

            migrationBuilder.DropTable(
                name: "NhaCungCap");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
