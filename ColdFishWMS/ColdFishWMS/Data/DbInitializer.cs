using ColdFishWMS.Data;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Models;

namespace ColdFishWMS.Data;

public static class DbInitializer
{
    public static void Initialize(ColdFishDbContext context)
    {
        // Đảm bảo cơ sở dữ liệu đã được tạo
        context.Database.EnsureCreated();

        // ==========================================
        // KHỞI TẠO USER & ROLE (DỮ LIỆU CỐ ĐỊNH)
        // ==========================================

        // 1. Seed Roles
        // 1. Seed Roles (Đảm bảo các Role luôn tồn tại)
        var definedRoles = new[]
        {
            new VaiTro{ TenVaiTro = AppRoles.QuanLyKho, MoTa = "Quản lý kho (Admin)" },
            new VaiTro{ TenVaiTro = AppRoles.NhanVienKho, MoTa = "Nhân viên kho" },
            new VaiTro{ TenVaiTro = AppRoles.KeToanKho, MoTa = "Kế toán kho" }
        };

        foreach (var role in definedRoles)
        {
            if (!context.VaiTros.Any(r => r.TenVaiTro == role.TenVaiTro))
            {
                 context.VaiTros.Add(role);
            }
        }
        context.SaveChanges();

        // 2. Seed Users
        if (!context.NguoiDungs.Any())
        {
            var adminRole = context.VaiTros.Single(r => r.TenVaiTro == AppRoles.QuanLyKho);
            var staffRole = context.VaiTros.Single(r => r.TenVaiTro == AppRoles.NhanVienKho);
            var accRole = context.VaiTros.Single(r => r.TenVaiTro == AppRoles.KeToanKho);

            var users = new NguoiDung[]
            {
                new NguoiDung {
                    TenDangNhap = "admin",
                    MatKhau = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    HoTen = "Administrator",
                    Email = "admin@coldfish.com",
                    SoDienThoai = "0123456789",
                    MaVaiTro = adminRole.MaVaiTro,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung {
                    TenDangNhap = "nhanvien",
                    MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"),
                    HoTen = "Nhân Viên Kho",
                    Email = "staff@coldfish.com",
                    SoDienThoai = "0900000001",
                    MaVaiTro = staffRole.MaVaiTro,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now
                },
                new NguoiDung {
                    TenDangNhap = "ketoan",
                    MatKhau = BCrypt.Net.BCrypt.HashPassword("123456"),
                    HoTen = "Kế Toán Kho",
                    Email = "accountant@coldfish.com",
                    SoDienThoai = "0900000002",
                    MaVaiTro = accRole.MaVaiTro,
                    TrangThaiHoatDong = true,
                    NgayTao = DateTime.Now
                }
            };
            context.NguoiDungs.AddRange(users);
            context.SaveChanges();
        }

        // ==========================================
        // KHỞI TẠO DỮ LIỆU DANH MỤC (MASTER DATA)
        // ==========================================
        
        // Kiểm tra nếu dữ liệu danh mục đã có thì không thêm nữa để tránh trùng lặp
        if (context.SanPhams.Any()) return; 

        // 3. Đơn vị tính
        var dvts = new DonViTinh[]
        {
            new DonViTinh{ TenDonViTinh = "Kg" },
            new DonViTinh{ TenDonViTinh = "Tấn" },
            new DonViTinh{ TenDonViTinh = "Thùng" },
            new DonViTinh{ TenDonViTinh = "Bao" },
            new DonViTinh{ TenDonViTinh = "Pallet" },
            new DonViTinh{ TenDonViTinh = "Vỉ" }
        };
        context.DonViTinhs.AddRange(dvts);
        context.SaveChanges();

        // 4. Vị trí kho (Tạo 2 kho A và B)
        var vitris = new List<ViTriKho>();
        for (int k = 1; k <= 3; k++) // 3 kệ
        {
            for (int t = 1; t <= 3; t++) // 3 tầng
            {
                vitris.Add(new ViTriKho { MaViTri = $"K-A-K{k}-T{t}", TenViTri = $"Kho A - Kệ {k} - Tầng {t}", Khu = "Kho Âm A", Ke = $"K{k}", Tang = $"{t}", TrangThaiTrong = true });
                vitris.Add(new ViTriKho { MaViTri = $"K-B-K{k}-T{t}", TenViTri = $"Kho B - Kệ {k} - Tầng {t}", Khu = "Kho Âm B", Ke = $"K{k}", Tang = $"{t}", TrangThaiTrong = true });
            }
        }
        context.ViTriKhos.AddRange(vitris);
        context.SaveChanges();

        // 5. Nhà cung cấp
        var nccs = new NhaCungCap[]
        {
            new NhaCungCap{ MaNhaCungCap = "NCC001", TenNhaCungCap = "Vĩnh Hoàn Corp", SoDienThoai = "02773852023", TrangThaiHoatDong = true },
            new NhaCungCap{ MaNhaCungCap = "NCC002", TenNhaCungCap = "Minh Phú Seafood", SoDienThoai = "02903848777", TrangThaiHoatDong = true },
            new NhaCungCap{ MaNhaCungCap = "NCC003", TenNhaCungCap = "Hùng Vương Corp", SoDienThoai = "02733854245", TrangThaiHoatDong = true },
            new NhaCungCap{ MaNhaCungCap = "NCC004", TenNhaCungCap = "Sao Ta Foods", SoDienThoai = "02993822122", TrangThaiHoatDong = true },
            new NhaCungCap{ MaNhaCungCap = "NCC005", TenNhaCungCap = "Navico Nam Việt", SoDienThoai = "02963852368", TrangThaiHoatDong = true }
        };
        context.NhaCungCaps.AddRange(nccs);
        context.SaveChanges();

        // 6. Khách hàng
        var khs = new KhachHang[]
        {
            new KhachHang{ MaKhachHang = "KH001", TenKhachHang = "BigC (GO!)", SoDienThoai = "19001880", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH002", TenKhachHang = "Co.op Mart", SoDienThoai = "1900555568", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH003", TenKhachHang = "WinMart", SoDienThoai = "02437106688", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH004", TenKhachHang = "Lotte Mart", SoDienThoai = "0912344444", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH005", TenKhachHang = "Aeon Mall", SoDienThoai = "1900888888", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH006", TenKhachHang = "Bach Hoa Xanh", SoDienThoai = "19001900", TrangThaiHoatDong = true },
            new KhachHang{ MaKhachHang = "KH007", TenKhachHang = "King Food", SoDienThoai = "0909999999", TrangThaiHoatDong = true }
        };
        context.KhachHangs.AddRange(khs);
        context.SaveChanges();

        // 7. Loại sản phẩm
        var loais = new LoaiSanPham[]
        {
            new LoaiSanPham{ TenLoai = "Cá Phi Lê", MoTa = "Các loại cá cắt phi lê cấp đông (Tra, Basa, Hồi...)" }, // Index 0
            new LoaiSanPham{ TenLoai = "Tôm", MoTa = "Tôm sú, tôm thẻ" },        // Index 1
            new LoaiSanPham{ TenLoai = "Hải Sản Khác", MoTa = "Mực, Bạch tuộc, Nghêu..." }   // Index 2
        };
        context.LoaiSanPhams.AddRange(loais);
        context.SaveChanges();
        
        // Reload để lấy ID
        var listLoai = context.LoaiSanPhams.ToList();
        var dvtKg = context.DonViTinhs.First(d => d.TenDonViTinh == "Kg");
        var dvtThung = context.DonViTinhs.First(d => d.TenDonViTinh == "Thùng");
        var dvtVi = context.DonViTinhs.First(d => d.TenDonViTinh == "Vỉ");

        // 8. Sản phẩm (7 Cá Phi Lê, 3 Khác)
        var sps = new SanPham[]
        {
            // 7 Sản phẩm Cá Phi Lê
            new SanPham{ MaSanPham = "SP001", TenSanPham = "Cá Tra Phi Lê (Túi 1kg)", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC001", GiaNhapMacDinh = 45000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP002", TenSanPham = "Cá Basa Phi Lê Cắt Khúc", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC001", GiaNhapMacDinh = 42000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP003", TenSanPham = "Cá Hồi Na Uy Phi Lê", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC002", GiaNhapMacDinh = 350000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP004", TenSanPham = "Cá Ngừ Đại Dương Phi Lê", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC002", GiaNhapMacDinh = 180000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP005", TenSanPham = "Cá Tuyết Phi Lê (Cod)", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC005", GiaNhapMacDinh = 250000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP006", TenSanPham = "Cá Minh Thái Phi Lê", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC005", GiaNhapMacDinh = 90000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP007", TenSanPham = "Cá Chẽm Phi Lê (Barramundi)", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[0].MaLoai, MaNhaCungCap = "NCC001", GiaNhapMacDinh = 120000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },

            // 3 Sản phẩm Khác
            // Tôm
            new SanPham{ MaSanPham = "SP008", TenSanPham = "Tôm Sú Nguyên Con (Hộp 500g)", MaDonViTinh = dvtThung.MaDonViTinh, MaLoai = listLoai[1].MaLoai, MaNhaCungCap = "NCC004", GiaNhapMacDinh = 180000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            // Mực & Khác
            new SanPham{ MaSanPham = "SP009", TenSanPham = "Mực Ống Làm Sạch", MaDonViTinh = dvtKg.MaDonViTinh, MaLoai = listLoai[2].MaLoai, MaNhaCungCap = "NCC003", GiaNhapMacDinh = 220000, TrangThaiHoatDong = true, NgayTao = DateTime.Now },
            new SanPham{ MaSanPham = "SP010", TenSanPham = "Bạch Tuộc Nhật (Vỉ)", MaDonViTinh = dvtVi.MaDonViTinh, MaLoai = listLoai[2].MaLoai, MaNhaCungCap = "NCC003", GiaNhapMacDinh = 90000, TrangThaiHoatDong = true, NgayTao = DateTime.Now }
        };
        context.SanPhams.AddRange(sps);
        context.SaveChanges();

        // ==========================================
        // KHỞI TẠO DỮ LIỆU GIAO DỊCH (TRANSACTIONS)
        // ==========================================

        if (context.PhieuNhaps.Any()) return; // Nếu đã có phiếu thì không tạo thêm

        var adminUser = context.NguoiDungs.First(u => u.TenDangNhap == "admin");
        var rnd = new Random();
        var today = DateTime.Now;

        // Xác định ngày bắt đầu từ ngày 1/10
        var startGenDate = new DateTime(today.Year, 10, 1);
        if (startGenDate > today) startGenDate = startGenDate.AddYears(-1);
        
        int daysToGen = (int)(today - startGenDate).TotalDays;

        // Theo dõi số lượng lô hàng trong mỗi vị trí (giả lập bộ nhớ tạm)
        var locationLoad = vitris.ToDictionary(v => v.MaViTri, v => 0);

        // 9. Tạo Phiếu Nhập (Từ 1/10 đến nay)
        for (int i = daysToGen; i >= 0; i--)
        {
            if (i % 2 == 0 || i % 5 == 0) // Nhập định kỳ
            {
                var date = today.AddDays(-i);
                var pnId = $"PN-{date:yyyyMMdd}-{rnd.Next(100, 999)}";
                var nccId = nccs[rnd.Next(nccs.Length)].MaNhaCungCap;

                var phieuNhap = new PhieuNhap
                {
                    MaPhieuNhap = pnId,
                    NgayNhap = date,
                    MaNhaCungCap = nccId,
                    MaNguoiTao = adminUser.MaNguoiDung,
                    GhiChu = $"Nhập hàng định kỳ ngày {date:dd/MM}",
                    DaDuyet = true,
                    TongTien = 0
                };
                
                decimal tongTien = 0;
                int numItems = rnd.Next(1, 4);
                
                for (int j = 0; j < numItems; j++)
                {
                    var sp = sps[rnd.Next(sps.Length)];
                    if (context.ChiTietPhieuNhaps.Local.Any(x => x.MaPhieuNhap == pnId && x.MaSanPham == sp.MaSanPham)) continue;

                    int qty = rnd.Next(10, 100) * 10; 
                    decimal price = sp.GiaNhapMacDinh;
                    
                    // -- LOGIC CHỌN VỊ TRÍ --
                    // Lọc các vị trí còn sức chứa (< 15 lô)
                    var availableLocs = vitris.Where(v => locationLoad[v.MaViTri] < 15).ToList();
                    
                    // Nếu full hết thì reset tạm hoặc chọn đại (tránh lỗi logic demo)
                    if (!availableLocs.Any()) availableLocs = vitris; 

                    var selectedLoc = availableLocs[rnd.Next(availableLocs.Count)];
                    
                    // Cập nhật trạng thái vị trí
                    locationLoad[selectedLoc.MaViTri]++;
                    selectedLoc.TrangThaiTrong = false; 

                    // Tạo lô hàng mới
                    var loId = $"LH-{date:yyyyMMdd}-{sp.MaSanPham.Substring(2)}-{rnd.Next(10,99)}";
                    var hsd = date.AddMonths(rnd.Next(6, 12)); 
                    
                    var loHang = new LoHang
                    {
                        MaLoHang = loId,
                        MaSanPham = sp.MaSanPham,
                        NgaySanXuat = date.AddDays(-5),
                        HanSuDung = hsd,
                        SoLuongNhap = qty,
                        SoLuongTon = qty, 
                        MaViTri = selectedLoc.MaViTri
                    };
                    context.LoHangs.Add(loHang);

                    var ctpn = new ChiTietPhieuNhap
                    {
                        MaPhieuNhap = pnId,
                        MaSanPham = sp.MaSanPham,
                        SoLuong = qty,
                        DonGia = price,
                        MaLoHang = loId
                    };
                    context.ChiTietPhieuNhaps.Add(ctpn);
                    tongTien += (qty * price);
                }
                phieuNhap.TongTien = tongTien;
                context.PhieuNhaps.Add(phieuNhap);
            }
        }
        context.SaveChanges(); 

        // 10. Tạo Phiếu Xuất (Từ 1/10 đến nay)
        var availLots = context.LoHangs.Where(l => l.SoLuongTon > 0).ToList();
        
        for (int i = daysToGen; i >= 0; i--) 
        {
            var date = today.AddDays(-i);
            
            if (rnd.NextDouble() > 0.4)
            {
                var pxId = $"PX-{date:yyyyMMdd}-{rnd.Next(100, 999)}";
                var khId = khs[rnd.Next(khs.Length)].MaKhachHang;

                var phieuXuat = new PhieuXuat
                {
                    MaPhieuXuat = pxId,
                    NgayXuat = date,
                    MaKhachHang = khId,
                    MaNguoiTao = adminUser.MaNguoiDung,
                    GhiChu = $"Xuất bán siêu thị {date:dd/MM}",
                    DaXuat = true,
                    TongTien = 0
                };

                decimal tongTien = 0;
                int numItems = rnd.Next(1, 5); 

                for (int j = 0; j < numItems; j++)
                {
                    if (!availLots.Any()) break;

                    var lot = availLots[rnd.Next(availLots.Count)];
                    
                    decimal qty = Math.Floor(lot.SoLuongTon * 0.1m * rnd.Next(1,6));
                    if (qty <= 0) qty = 1;
                    if (qty > lot.SoLuongTon) qty = lot.SoLuongTon;

                    decimal price = lot.SanPham.GiaNhapMacDinh * 1.3m; 

                    var ctpx = new ChiTietPhieuXuat
                    {
                        MaPhieuXuat = pxId,
                        MaSanPham = lot.MaSanPham,
                        SoLuong = qty,
                        DonGia = price,
                        MaLoHang = lot.MaLoHang
                    };
                    context.ChiTietPhieuXuats.Add(ctpx);

                    lot.SoLuongTon -= qty;
                    if (lot.SoLuongTon <= 0) availLots.Remove(lot); 
                    
                    tongTien += (qty * price);
                }
                phieuXuat.TongTien = tongTien;
                context.PhieuXuats.Add(phieuXuat);
            }
        }
        
        // 11. Cập nhật lại Trạng Thái Vị Trí cuối cùng
        // Kiểm tra thực tế: Vị trí nào còn lô hàng có SoLuongTon > 0 thì là false (Có hàng), ngược lại true (Trống)
        foreach (var vt in vitris)
        {
            // Lưu ý: Local check vì data chưa commit hết hoặc vừa add
            bool hasStock = context.LoHangs.Local.Any(l => l.MaViTri == vt.MaViTri && l.SoLuongTon > 0);
            vt.TrangThaiTrong = !hasStock;
        }

        context.SaveChanges();
    }
}
