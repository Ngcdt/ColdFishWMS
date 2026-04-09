using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ColdFishWMS.Migrations
{
    /// <inheritdoc />
    public partial class AddNhatKyNhietDo_FixFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyNhietDos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NhietDo = table.Column<double>(type: "float", nullable: false),
                    DoAm = table.Column<double>(type: "float", nullable: true),
                    ThoiGianGhi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaThietBi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaViTri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyNhietDos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyNhietDos_ViTriKho_MaViTri",
                        column: x => x.MaViTri,
                        principalTable: "ViTriKho",
                        principalColumn: "MaViTri");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyNhietDos_MaViTri",
                table: "NhatKyNhietDos",
                column: "MaViTri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyNhietDos");
        }
    }
}
