using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ColdFishWMS.Data;
using ColdFishWMS.Models;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Models.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Chart;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.KeToanKho)]
public class BaoCaoController : Controller
{
    private readonly ColdFishDbContext _context;

    public BaoCaoController(ColdFishDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string period = "month", string reportType = "all", string searchLocation = "")
    {
         // Period Selection Logic
         var (from, to) = CalculateDateRange(period, startDate, endDate);

         var model = new BaoCaoViewModel
         {
             FromDate = from,
             ToDate = to
         };

         ViewBag.CurrentPeriod = period;
         ViewBag.CurrentReportType = reportType;
         ViewBag.CurrentLocation = searchLocation;
         
         // Fetch Locations for Dropdown
         ViewBag.Locations = await _context.ViTriKhos.OrderBy(v => v.MaViTri).ToListAsync();

         // 1. CHART DATA GENERATION (Dynamic based on Period)
         model.ChartLabels = new List<string>();
         model.ChartDataImport = new List<decimal>();
         model.ChartDataExport = new List<decimal>();

         if (period == "year") // Monthly breakdown for the year
         {
             for (int i = 1; i <= 12; i++)
             {
                 var monthStart = new DateTime(from.Year, i, 1);
                 var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
                 
                 // Skip future months if you want, or show 0
                 model.ChartLabels.Add($"T{i}");
                 model.ChartDataImport.Add(await _context.PhieuNhaps.Where(p => p.NgayNhap >= monthStart && p.NgayNhap <= monthEnd).SumAsync(p => p.TongTien));
                 model.ChartDataExport.Add(await _context.PhieuXuats.Where(p => p.NgayXuat >= monthStart && p.NgayXuat <= monthEnd).SumAsync(p => p.TongTien));
             }
         }
         else if (period == "month") // Daily breakdown for the month
         {
             var daysInMonth = DateTime.DaysInMonth(from.Year, from.Month);
             for (int i = 1; i <= daysInMonth; i++)
             {
                 var date = new DateTime(from.Year, from.Month, i);
                 model.ChartLabels.Add($"{i}/{from.Month}");
                 model.ChartDataImport.Add(await _context.PhieuNhaps.Where(p => p.NgayNhap.Date == date).SumAsync(p => p.TongTien));
                 model.ChartDataExport.Add(await _context.PhieuXuats.Where(p => p.NgayXuat.Date == date).SumAsync(p => p.TongTien));
             }
         }
         else // Default (Week/Today/Custom) - Daily breakdown within range
         {
             // Limit to max 31 bars to prevent overcrowding
             var daysDiff = (to - from).Days + 1;
             if (daysDiff <= 31)
             {
                 for (int i = 0; i < daysDiff; i++)
                 {
                     var date = from.AddDays(i).Date;
                     model.ChartLabels.Add(date.ToString("dd/MM"));
                     model.ChartDataImport.Add(await _context.PhieuNhaps.Where(p => p.NgayNhap.Date == date).SumAsync(p => p.TongTien));
                     model.ChartDataExport.Add(await _context.PhieuXuats.Where(p => p.NgayXuat.Date == date).SumAsync(p => p.TongTien));
                 }
             }
             else
             {
                 // Too many days, maybe aggregate by week? Let's stick to Month view logic for simplified handling for now
                 // Fallback: Just show Total Import vs Total Export as 2 columns? 
                 // Better: Show weekly chunks.
                 model.ChartLabels.Add("Tổng kỳ");
                 model.ChartDataImport.Add(await _context.PhieuNhaps.Where(p => p.NgayNhap >= from && p.NgayNhap <= to).SumAsync(p => p.TongTien));
                 model.ChartDataExport.Add(await _context.PhieuXuats.Where(p => p.NgayXuat >= from && p.NgayXuat <= to).SumAsync(p => p.TongTien));
             }
         }

         // 1.5. CHART DATA: PIE (Tồn kho theo danh mục) & BAR (Top sản phẩm xuất)
         
         // A. Pie Chart: Inventory Value by Category
         var inventoryData = await _context.LoHangs
             .Include(l => l.SanPham).ThenInclude(sp => sp.LoaiSanPham)
             .Include(l => l.ChiTietPhieuNhaps)
             .Where(l => l.SoLuongTon > 0)
             .ToListAsync();

         var inventoryByCategory = inventoryData
             .GroupBy(l => l.SanPham.LoaiSanPham?.TenLoai ?? "Khác")
             .Select(g => new
             {
                 Category = g.Key,
                 // Priority: Price from Batch (Import Detail) -> Default Import Price
                 Value = g.Sum(l => l.SoLuongTon * (l.ChiTietPhieuNhaps.FirstOrDefault()?.DonGia ?? l.SanPham.GiaNhapMacDinh))
             })
             .OrderByDescending(x => x.Value)
             .ToList();

         model.PieChartLabels = inventoryByCategory.Select(x => x.Category).ToList();
         model.PieChartData = inventoryByCategory.Select(x => x.Value).ToList();

         // B. Bar Chart: Top 5 Best Selling Products (Revenue)
         var topExports = await _context.ChiTietPhieuXuats
             .Include(x => x.PhieuXuat)
             .Include(x => x.SanPham)
             .Where(x => x.PhieuXuat.NgayXuat >= from && x.PhieuXuat.NgayXuat <= to)
             .GroupBy(x => x.SanPham.TenSanPham)
             .Select(g => new
             {
                 Product = g.Key,
                 Revenue = g.Sum(x => x.SoLuong * x.DonGia)
             })
             .OrderByDescending(x => x.Revenue)
             .Take(5)
             .ToListAsync();

         model.BarChartLabels = topExports.Select(x => x.Product).ToList();
         model.BarChartData = topExports.Select(x => x.Revenue).ToList();

         // 2. FEFO Stats
         model.SpoilageCount = await _context.LoHangs.CountAsync(l => l.HanSuDung < DateTime.Today && l.SoLuongTon > 0);
         model.FefoPercentage = 100; // Simplified

         // 3. Report Details - Filtered by reportType
         var items = new List<ReportDetailItem>();

         if (reportType == "tonkho")
         {
             var query = _context.LoHangs
                 .Include(l => l.SanPham)
                 .Include(l => l.ViTriKho)
                 .Where(l => l.SoLuongTon > 0);
            
             if (!string.IsNullOrEmpty(searchLocation))
             {
                 query = query.Where(l => l.ViTriKho.MaViTri == searchLocation);
             }

             var inventory = await query.ToListAsync();

             foreach (var l in inventory)
             {
                 items.Add(new ReportDetailItem
                 {
                     Date = l.HanSuDung, // For Inventory, show Expiry Date
                     ProductName = l.SanPham?.TenSanPham ?? "N/A",
                     BatchCode = l.MaLoHang,
                     Type = "Tồn kho",
                     Quantity = l.SoLuongTon,
                     ClassType = "badge bg-warning bg-opacity-10 text-warning"
                 });
             }
         }
         else
         {
             // Import / Export / All
             if (reportType == "all" || reportType == "nhapkho")
             {
                 var query = _context.ChiTietPhieuNhaps
                     .Include(x => x.PhieuNhap)
                     .Include(x => x.SanPham)
                     .Include(x => x.LoHang) // Contains ViTriKho via LoHang
                     .ThenInclude(l => l.ViTriKho)
                     .Where(x => x.PhieuNhap.NgayNhap >= from && x.PhieuNhap.NgayNhap <= to);

                 if (!string.IsNullOrEmpty(searchLocation))
                 {
                     query = query.Where(x => x.LoHang.ViTriKho.MaViTri == searchLocation);
                 }

                 var imports = await query.ToListAsync();
                 
                 items.AddRange(imports.Select(i => new ReportDetailItem {
                     Date = i.PhieuNhap.NgayNhap,
                     ProductName = i.SanPham?.TenSanPham ?? "N/A",
                     BatchCode = i.LoHang?.MaLoHang ?? "N/A",
                     Type = "Nhập kho",
                     Quantity = i.SoLuong,
                     ClassType = "badge bg-success bg-opacity-10 text-success"
                 }));
             }

             if (reportType == "all" || reportType == "xuatkho")
             {
                 var query = _context.ChiTietPhieuXuats
                     .Include(x => x.PhieuXuat)
                     .Include(x => x.SanPham)
                     .Include(x => x.LoHang)
                     .ThenInclude(l => l.ViTriKho)
                     .Where(x => x.PhieuXuat.NgayXuat >= from && x.PhieuXuat.NgayXuat <= to);

                  if (!string.IsNullOrEmpty(searchLocation))
                  {
                      query = query.Where(x => x.LoHang.ViTriKho.MaViTri == searchLocation);
                  }

                 var exports = await query.ToListAsync();

                 items.AddRange(exports.Select(e => new ReportDetailItem {
                     Date = e.PhieuXuat.NgayXuat,
                     ProductName = e.SanPham?.TenSanPham ?? "N/A",
                     BatchCode = e.LoHang?.MaLoHang ?? "N/A",
                     Type = "Xuất kho",
                     Quantity = e.SoLuong,
                     ClassType = "badge bg-danger bg-opacity-10 text-danger"
                 }));
             }
         }

         model.ReportDetails = items.OrderByDescending(x => x.Date).ToList();

        return View(model);
    }

    // --- ACTIONS EXPORT & PREVIEW ---

    public async Task<IActionResult> ExportImport(DateTime? startDate, DateTime? endDate)
    {
        var data = await GetImportData(startDate, endDate);
        
        var summary = data
            .GroupBy(x => x.NhaCungCap)
            .Select(g => new SummaryTableItem { Name = g.Key, Value = g.Sum(x => x.ThanhTien) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        return GenerateExcel(data, "NhapKho", "KH", "BÁO CÁO NHẬP KHO CHI TIẾT", startDate, endDate, summary, "TOP 5 NHÀ CUNG CẤP");
    }

    public async Task<IActionResult> ExportExport(DateTime? startDate, DateTime? endDate)
    {
        var data = await GetExportData(startDate, endDate);
        
        var summary = data
            .GroupBy(x => x.KhachHang)
            .Select(g => new SummaryTableItem { Name = g.Key, Value = g.Sum(x => x.ThanhTien) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        return GenerateExcel(data, "XuatKho", "XK", "BÁO CÁO XUẤT KHO CHI TIẾT", startDate, endDate, summary, "TOP 5 KHÁCH HÀNG");
    }

    public async Task<IActionResult> ExportInventory(DateTime? startDate, DateTime? endDate)
    {
        var data = await GetInventoryData();
        
        DateTime minDate;
        DateTime maxDate;

        if (startDate.HasValue) 
            minDate = startDate.Value;
        else 
        {
             minDate = DateTime.Now; // Default fallback
             if (data.Any())
             {
                 var earliest = data.Min(x => x.NgayNhap); 
                 if (earliest != DateTime.MinValue) minDate = earliest;
                 else if (data.Any(x => x.NgaySanXuat != DateTime.MinValue)) minDate = data.Min(x => x.NgaySanXuat);
             }
        }

        if (endDate.HasValue) maxDate = endDate.Value;
        else maxDate = DateTime.Now;

        var summaryData = await _context.LoHangs
             .Include(l => l.SanPham).ThenInclude(sp => sp.LoaiSanPham)
             .Where(l => l.SoLuongTon > 0)
             .GroupBy(l => l.SanPham.LoaiSanPham.TenLoai)
             .Select(g => new SummaryTableItem { 
                 Name = g.Key ?? "Khác", 
                 Value = g.Sum(l => l.SoLuongTon * l.SanPham.GiaNhapMacDinh) 
             })
             .OrderByDescending(x => x.Value)
             .ToListAsync();

        return GenerateExcel(data, "TonKho", "TK", "BÁO CÁO TỒN KHO CHI TIẾT", minDate, maxDate, summaryData, "CƠ CẤU TỒN KHO");
    }





    public async Task<IActionResult> PreviewInventory(string period, DateTime? startDate, DateTime? endDate)
    {
        var (from, to) = CalculateDateRange(period, startDate, endDate);
        var data = await GetInventoryData();
        // Chart: Inventory Value by Category 
        var inventoryChartData = await _context.LoHangs
             .Include(l => l.SanPham).ThenInclude(sp => sp.LoaiSanPham)
             .Where(l => l.SoLuongTon > 0)
             .GroupBy(l => l.SanPham.LoaiSanPham.TenLoai)
             .Select(g => new { 
                 Category = g.Key ?? "Khác", 
                 Value = g.Sum(l => l.SoLuongTon * l.SanPham.GiaNhapMacDinh) // Simplified valuation for preview speed
             })
             .OrderByDescending(x => x.Value)
             .ToListAsync();

        return PartialView("_ReportPreview", new ColdFishWMS.Models.ViewModels.PreviewReportViewModel 
        { 
            Title = "Chi tiết Tồn kho", 
            Type = "Inventory", 
            StartDate = from,
            EndDate = to,
            DataInventory = data,
            ChartTitle = "Cơ cấu giá trị tồn kho (Theo danh mục)",
            ChartLabels = inventoryChartData.Select(x => x.Category).ToList(),
            ChartData = inventoryChartData.Select(x => x.Value).ToList()
        });
    }

    public async Task<IActionResult> ExportNXT(DateTime? startDate, DateTime? endDate)
    {
        var from = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to = endDate ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

        // Fetch all history needed for accurate Opening Stock calculation
        var allImports = await _context.ChiTietPhieuNhaps
            .Include(x => x.PhieuNhap)
            .Include(x => x.LoHang)
            .Include(x => x.SanPham).ThenInclude(sp => sp.DonViTinh)
            .ToListAsync();

        var allExports = await _context.ChiTietPhieuXuats
            .Include(x => x.PhieuXuat)
            .Include(x => x.LoHang).ThenInclude(lh => lh.ChiTietPhieuNhaps) // To find Cost Price
            .Include(x => x.SanPham)
            .ToListAsync();

        var products = await _context.SanPhams.Include(sp => sp.DonViTinh).OrderBy(p => p.MaSanPham).ToListAsync();

        // Set License Context for EPPlus 5+
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var sheet = package.Workbook.Worksheets.Add("NXT");

            // --- HEADERS ---
            // Title
            var titleRange = sheet.Cells["A1:L1"];
            titleRange.Merge = true;
            titleRange.Value = "BÁO CÁO NHẬP - XUẤT - TỒN KHO HÀNG HÓA";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 16;
            titleRange.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            var dateRange = sheet.Cells["A2:L2"];
            dateRange.Merge = true;
            dateRange.Value = $"Từ ngày {from:dd/MM/yyyy} đến ngày {to:dd/MM/yyyy}";
            dateRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Header Row 1 (Merged Categories)
            int h1 = 3;
            sheet.Cells[h1, 1].Value = "Mã sản phẩm"; sheet.Cells[h1, 1, h1+1, 1].Merge = true;
            sheet.Cells[h1, 2].Value = "Tên sản phẩm"; sheet.Cells[h1, 2, h1+1, 2].Merge = true;
            sheet.Cells[h1, 3].Value = "ĐVT"; sheet.Cells[h1, 3, h1+1, 3].Merge = true;

            sheet.Cells[h1, 4].Value = "Tồn đầu kỳ"; sheet.Cells[h1, 4, h1, 5].Merge = true;
            sheet.Cells[h1, 6].Value = "Nhập trong kỳ"; sheet.Cells[h1, 6, h1, 7].Merge = true;
            sheet.Cells[h1, 8].Value = "Xuất trong kỳ"; sheet.Cells[h1, 8, h1, 10].Merge = true; 
            sheet.Cells[h1, 11].Value = "Tồn cuối kỳ"; sheet.Cells[h1, 11, h1, 12].Merge = true;
            
            // Header Row 2 (Sub-columns)
            int h2 = 4;
            sheet.Cells[h2, 4].Value = "Số lượng"; sheet.Cells[h2, 5].Value = "Thành tiền";
            sheet.Cells[h2, 6].Value = "Số lượng"; sheet.Cells[h2, 7].Value = "Thành tiền";
            sheet.Cells[h2, 8].Value = "Đơn giá xuất"; sheet.Cells[h2, 9].Value = "Số lượng"; sheet.Cells[h2, 10].Value = "Thành tiền";
            sheet.Cells[h2, 11].Value = "Số lượng"; sheet.Cells[h2, 12].Value = "Thành tiền";

            // Header Style
            // Header Style
            var headerRange = sheet.Cells[h1, 1, h2, 12];
            headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // 1. Info (Cols 1-3) - Dark Gray/Blue
            sheet.Cells[h1, 1, h2, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[h1, 1, h2, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(44, 62, 80)); 

            // 2. Opening Stock (Cols 4-5) - Blue
            sheet.Cells[h1, 4, h2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[h1, 4, h2, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));

            // 3. Import (Cols 6-7) - Green
            sheet.Cells[h1, 6, h2, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[h1, 6, h2, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(39, 174, 96));

            // 4. Export (Cols 8-10) - Orange
            sheet.Cells[h1, 8, h2, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[h1, 8, h2, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(211, 84, 0));

            // 5. Closing Stock (Cols 11-12) - Purple
            sheet.Cells[h1, 11, h2, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[h1, 11, h2, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(142, 68, 173));

            // Borders
            headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            // --- DATA CALCULATION & FILL ---
            var chartDataList = new List<dynamic>();
            decimal tQtyOpen = 0, tValOpen = 0;
            decimal tQtyImp = 0, tValImp = 0;
            decimal tQtyExp = 0, tValExp = 0;
            decimal tQtyClose = 0, tValClose = 0;
            int row = 5;
            foreach (var p in products)
            {
                var pImports = allImports.Where(x => x.MaSanPham == p.MaSanPham).ToList();
                var pExports = allExports.Where(x => x.MaSanPham == p.MaSanPham).ToList();

                // 1. OPENING STOCK (Before 'from' date)
                var impOpen = pImports.Where(x => x.PhieuNhap.NgayNhap < from).ToList();
                var expOpen = pExports.Where(x => x.PhieuXuat.NgayXuat < from).ToList();

                decimal qtyImpOpen = impOpen.Sum(x => x.SoLuong);
                decimal valImpOpen = impOpen.Sum(x => x.SoLuong * x.DonGia);

                decimal qtyExpOpen = expOpen.Sum(x => x.SoLuong);
                decimal valExpOpen = expOpen.Sum(x => {
                    // Cost Price logic: Best effort to find original import price of the batch
                    var costPrice = x.LoHang?.ChiTietPhieuNhaps.FirstOrDefault()?.DonGia ?? 0;
                    return x.SoLuong * costPrice;
                });

                decimal qtyOpen = qtyImpOpen - qtyExpOpen;
                decimal valOpen = valImpOpen - valExpOpen;

                // 2. IN PERIOD
                var impPeriod = pImports.Where(x => x.PhieuNhap.NgayNhap >= from && x.PhieuNhap.NgayNhap <= to).ToList();
                var expPeriod = pExports.Where(x => x.PhieuXuat.NgayXuat >= from && x.PhieuXuat.NgayXuat <= to).ToList();

                decimal qtyImp = impPeriod.Sum(x => x.SoLuong);
                decimal valImp = impPeriod.Sum(x => x.SoLuong * x.DonGia);

                decimal qtyExp = expPeriod.Sum(x => x.SoLuong);
                decimal valExp = expPeriod.Sum(x => {
                    var costPrice = x.LoHang?.ChiTietPhieuNhaps.FirstOrDefault()?.DonGia ?? 0;
                    return x.SoLuong * costPrice;
                });

                // Average Export Price
                decimal avgExpPrice = qtyExp > 0 ? Math.Round(valExp / qtyExp, 0) : 0;

                // 3. CLOSING STOCK
                decimal qtyClose = qtyOpen + qtyImp - qtyExp;
                decimal valClose = valOpen + valImp - valExp;

                // Filter: Show if any activity exists OR non-zero stock
                bool hasActivity = qtyImp > 0 || qtyExp > 0;
                bool hasStock = qtyOpen > 0 || qtyClose > 0;

                if (!hasActivity && !hasStock) continue;

                sheet.Cells[row, 1].Value = p.MaSanPham;
                sheet.Cells[row, 2].Value = p.TenSanPham;
                sheet.Cells[row, 3].Value = p.DonViTinh?.TenDonViTinh;

                sheet.Cells[row, 4].Value = qtyOpen;
                sheet.Cells[row, 5].Value = valOpen;

                sheet.Cells[row, 6].Value = qtyImp;
                sheet.Cells[row, 7].Value = valImp;

                sheet.Cells[row, 8].Value = avgExpPrice;
                sheet.Cells[row, 9].Value = qtyExp;
                sheet.Cells[row, 10].Value = valExp;

                sheet.Cells[row, 11].Value = qtyClose;
                sheet.Cells[row, 12].Value = valClose;

                // Accumulate
                tQtyOpen += qtyOpen; tValOpen += valOpen;
                tQtyImp += qtyImp; tValImp += valImp;
                tQtyExp += qtyExp; tValExp += valExp;
                tQtyClose += qtyClose; tValClose += valClose;

                // Collect for Chart (Top Activity)
                if (hasActivity)
                {
                    chartDataList.Add(new { Name = p.TenSanPham, Import = valImp, Export = valExp, Total = valImp + valExp });
                }

                row++;
            }

            // Borders for data
            var dataRange = sheet.Cells[5, 1, row - 1, 12];
            dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            
            // Format Numbers
            sheet.Cells[5, 4, row - 1, 12].Style.Numberformat.Format = "#,##0";
            sheet.Cells[5, 1, row - 1, 12].AutoFitColumns();

            // --- SUMMARY & CHART (NXT) ---
            var top5 = chartDataList.OrderByDescending(x => (decimal)x.Total).Take(5).ToList();
            if (top5.Any())
            {
                int sumRow = 3;
                int sumCol = 14; // Column N
                
                // Header
                var sumHeader = sheet.Cells[sumRow, sumCol, sumRow, sumCol + 2];
                sumHeader.Merge = true;
                sumHeader.Value = "TOP 5 SẢN PHẨM HOẠT ĐỘNG (NHẬP + XUẤT)";
                sumHeader.Style.Font.Bold = true;
                sumHeader.Style.Fill.PatternType = ExcelFillStyle.Solid;
                sumHeader.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sumHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                sheet.Cells[sumRow + 1, sumCol].Value = "Sản phẩm";
                sheet.Cells[sumRow + 1, sumCol + 1].Value = "Giá trị Nhập";
                sheet.Cells[sumRow + 1, sumCol + 2].Value = "Giá trị Xuất";
                
                var subHeader = sheet.Cells[sumRow + 1, sumCol, sumRow + 1, sumCol + 2];
                subHeader.Style.Font.Bold = true;
                subHeader.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                subHeader.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                int sRow = sumRow + 2;
                foreach (var item in top5)
                {
                    sheet.Cells[sRow, sumCol].Value = item.Name;
                    sheet.Cells[sRow, sumCol + 1].Value = item.Import;
                    sheet.Cells[sRow, sumCol + 2].Value = item.Export;
                    sRow++;
                }
                
                // Format Summary
                var sumRange = sheet.Cells[sumRow + 2, sumCol + 1, sRow - 1, sumCol + 2];
                sumRange.Style.Numberformat.Format = "#,##0";
                sheet.Cells[sumRow, sumCol, sRow - 1, sumCol + 2].AutoFitColumns();
                sheet.Cells[sumRow, sumCol, sRow - 1, sumCol + 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                // Chart: Clustered Column
                var chart = sheet.Drawings.AddChart("ChartNXT", eChartType.ColumnClustered);
                chart.SetPosition(sRow + 1, 0, sumCol - 1, 0); 
                chart.SetSize(600, 400);
                chart.Title.Text = "TOP 5 SẢN PHẨM - NHẬP & XUẤT";
                
                // Data Ranges
                var series1 = chart.Series.Add(sheet.Cells[sumRow + 2, sumCol + 1, sRow - 1, sumCol + 1], sheet.Cells[sumRow + 2, sumCol, sRow - 1, sumCol]);
                series1.Header = "Nhập";
                
                var series2 = chart.Series.Add(sheet.Cells[sumRow + 2, sumCol + 2, sRow - 1, sumCol + 2], sheet.Cells[sumRow + 2, sumCol, sRow - 1, sumCol]);
                series2.Header = "Xuất";

                // Data Labels (Dynamic Bypass)
                dynamic s1 = series1; s1.DataLabel.ShowValue = true; s1.DataLabel.Position = eLabelPosition.OutEnd;
                dynamic s2 = series2; s2.DataLabel.ShowValue = true; s2.DataLabel.Position = eLabelPosition.OutEnd;

                chart.Style = eChartStyle.Style2;
            }

            // TOTAL ROW
            sheet.Cells[row, 1, row, 12].Style.Font.Bold = true;
            sheet.Cells[row, 1, row, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
            sheet.Cells[row, 1, row, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            sheet.Cells[row, 1, row, 12].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            // Internal borders for total row
            sheet.Cells[row, 1, row, 12].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            sheet.Cells[row, 1, row, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            sheet.Cells[row, 2].Value = "TỔNG CỘNG";
            sheet.Cells[row, 2, row, 3].Merge = true;
            sheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            // Set Values
            sheet.Cells[row, 4].Value = tQtyOpen;
            sheet.Cells[row, 5].Value = tValOpen;
            sheet.Cells[row, 6].Value = tQtyImp;
            sheet.Cells[row, 7].Value = tValImp;
            
            // Col 8 (Avg Price) - maybe avg of total? or empty? Usually empty for Total row as avg of totals is meaningless or weighted.
            // Leaving empty or 0.
            
            sheet.Cells[row, 9].Value = tQtyExp;
            sheet.Cells[row, 10].Value = tValExp;
            sheet.Cells[row, 11].Value = tQtyClose;
            sheet.Cells[row, 12].Value = tValClose;
            
            sheet.Cells[row, 4, row, 12].Style.Numberformat.Format = "#,##0";

            sheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCaoNXT_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }
    }

    private string GetColName(int colIndex)
    {
        int dividend = colIndex;
        string columnName = String.Empty;
        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
            dividend = (int)((dividend - modulo) / 26);
        }
        return columnName;
    }

    // --- PREVIEW ACTIONS ---

    public async Task<IActionResult> PreviewImport(string period, DateTime? startDate, DateTime? endDate)
    {
        var (from, to) = CalculateDateRange(period, startDate, endDate);
        var data = await GetImportData(from, to);
        // Chart: Top Suppliers by Import Value
        var chartData = data
            .GroupBy(x => x.NhaCungCap)
            .Select(g => new { Name = g.Key, Value = g.Sum(x => x.ThanhTien) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        return PartialView("_ReportPreview", new ColdFishWMS.Models.ViewModels.PreviewReportViewModel 
        { 
            Title = "Chi tiết Nhập kho", 
            Type = "Import", 
            StartDate = from,
            EndDate = to,
            DataImport = data,
            ChartTitle = "Top 5 Nhà cung cấp (Theo giá trị nhập)",
            ChartLabels = chartData.Select(x => x.Name).ToList(),
            ChartData = chartData.Select(x => x.Value).ToList()
        });
    }

    public async Task<IActionResult> PreviewExport(string period, DateTime? startDate, DateTime? endDate)
    {
        var (from, to) = CalculateDateRange(period, startDate, endDate);
        var data = await GetExportData(from, to);
        // Chart: Top Customers by Export Value
        var chartData = data
            .GroupBy(x => x.KhachHang)
            .Select(g => new { Name = g.Key, Value = g.Sum(x => x.ThanhTien) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        return PartialView("_ReportPreview", new ColdFishWMS.Models.ViewModels.PreviewReportViewModel 
        { 
            Title = "Chi tiết Xuất kho", 
            Type = "Export", 
            StartDate = from,
            EndDate = to,
            DataExport = data,
            ChartTitle = "Top 5 Khách hàng (Theo giá trị xuất)",
            ChartLabels = chartData.Select(x => x.Name).ToList(),
            ChartData = chartData.Select(x => x.Value).ToList()
        });
    }



    // --- PRIVATE HELPERS ---

    public class SummaryTableItem
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }

    private IActionResult GenerateExcel<T>(IEnumerable<T> data, string sheetName, string prefix, string title, DateTime? fromDate, DateTime? toDate, List<SummaryTableItem> summaryData = null, string summaryTitle = "")
    {
        // Set License Context for EPPlus 5+
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            
            // 1. HEADER (Title & Date)
            var titleRange = worksheet.Cells["A1:H1"];
            titleRange.Merge = true;
            titleRange.Value = title.ToUpper();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.Size = 14;
            titleRange.Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            var timeRange = worksheet.Cells["A2:H2"];
            if (fromDate.HasValue && toDate.HasValue)
            {
                 timeRange.Merge = true;
                 timeRange.Value = $"Từ ngày {fromDate.Value:dd/MM/yyyy} đến ngày {toDate.Value:dd/MM/yyyy}";
            }
            else
            {
                 timeRange.Merge = true;
                 timeRange.Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy}";
            }
            timeRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // 2. MAIN DATA TABLE (Starts at row 4)
            // LoadFromCollection automatically handles headers
            var tableRange = worksheet.Cells["A4"].LoadFromCollection(data, true, OfficeOpenXml.Table.TableStyles.Medium2);
            
            // Fix Date Formats
            var headerRow = worksheet.Cells[4, 1, 4, tableRange.Columns];
            for (int i = 1; i <= tableRange.Columns; i++)
            {
                var header = worksheet.Cells[4, i].Text;
                if (header.Contains("Ngay") || header.Contains("Han") || header.Contains("Date"))
                {
                    worksheet.Column(i).Style.Numberformat.Format = "dd/MM/yyyy";
                    worksheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                else if (header.Contains("Gia") || header.Contains("Tien") || header.Contains("Value"))
                {
                    worksheet.Column(i).Style.Numberformat.Format = "#,##0";
                }
            }

            // 3. SIDE SUMMARY TABLE & CHART (Starts at M4 - Gap of 2 columns)
            if (summaryData != null && summaryData.Any())
            {
                int tableEndCol = tableRange.End.Column; // Find where table ends
                int summaryCol = tableEndCol + 2; // Leave 1 empty column gap
                int summaryRow = 4;

                // Title for Summary
                var summaryTitleRange = worksheet.Cells[summaryRow, summaryCol, summaryRow, summaryCol + 1];
                summaryTitleRange.Merge = true;
                summaryTitleRange.Value = summaryTitle;
                summaryTitleRange.Style.Font.Bold = true;
                summaryTitleRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                summaryTitleRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                summaryTitleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                summaryTitleRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                summaryTitleRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                summaryTitleRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                summaryTitleRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Table Header
                worksheet.Cells[summaryRow + 1, summaryCol].Value = "Nội dung";
                worksheet.Cells[summaryRow + 1, summaryCol + 1].Value = "Giá trị";
                worksheet.Cells[summaryRow + 1, summaryCol, summaryRow + 1, summaryCol + 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow + 1, summaryCol, summaryRow + 1, summaryCol + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Data
                int r = summaryRow + 2;
                foreach (var item in summaryData)
                {
                    worksheet.Cells[r, summaryCol].Value = item.Name;
                    worksheet.Cells[r, summaryCol + 1].Value = item.Value;
                    worksheet.Cells[r, summaryCol + 1].Style.Numberformat.Format = "#,##0";
                    r++;
                }
                
                // Border for Summary Table
                var sumTableRange = worksheet.Cells[summaryRow + 1, summaryCol, r - 1, summaryCol + 1];
                sumTableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                sumTableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                sumTableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                sumTableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                
                worksheet.Column(summaryCol).AutoFit();
                worksheet.Column(summaryCol + 1).AutoFit();

                // 4. CREATING NATIVE EXCEL CHART
                var chartDataRange = worksheet.Cells[summaryRow + 2, summaryCol + 1, r - 1, summaryCol + 1]; // Values
                var chartLabelRange = worksheet.Cells[summaryRow + 2, summaryCol, r - 1, summaryCol]; // Names
                
                // Determine Chart Position relative to Table
                // Table ends at row 'r'. Let's add chart at row r + 1.
                // 0-based index for SetPosition: Row = r (since r is 1-based, r is the next row index).
                int chartRowIndex = r; 
                int chartColIndex = summaryCol - 1; // Align with Summary Table (0-based)

                ExcelChart chart;
                if (summaryTitle.Contains("TỒN KHO")) // Inventory -> Pie Chart
                {
                    chart = worksheet.Drawings.AddChart("ChartSummary", eChartType.Pie3D);
                    chart.Title.Text = summaryTitle;
                    var series = chart.Series.Add(chartDataRange, chartLabelRange);
                    
                    // Pie Chart Labels
                    // Use dynamic to bypass compile-time check for DataLabel if strict type is missing it
                    dynamic ser = series; 
                    ser.DataLabel.ShowCategory = true;
                    ser.DataLabel.ShowPercent = true;
                    ser.DataLabel.Position = eLabelPosition.OutEnd; 
                    
                    chart.Style = eChartStyle.Style18;
                }
                else // Import/Export -> Bar Chart (Horizontal for better label visibility)
                {
                    chart = worksheet.Drawings.AddChart("ChartSummary", eChartType.BarClustered);
                    chart.Title.Text = summaryTitle;
                    var series = chart.Series.Add(chartDataRange, chartLabelRange);
                    
                    // Bar Chart Labels
                    series.Header = "Giá trị (VNĐ)";
                    
                    dynamic ser = series;
                    ser.DataLabel.ShowValue = true;
                    ser.DataLabel.Position = eLabelPosition.OutEnd;
                    
                    chart.Legend.Remove(); // Value is clear from labels/axis
                    chart.Style = eChartStyle.Style2;
                }

                chart.SetPosition(chartRowIndex, 10, chartColIndex, 0); // Add 10px top margin
                chart.SetSize(400, 300); // Slightly smaller to fit column width better? Or keep large. 
                // Let's make it match the table visual width approx? 
                // Table is 2 columns wide. Chart needs to be wider. 
                // Let's keep it 500x350 but centered or left aligned?
                // Align left with table is fine.
                chart.SetSize(500, 350);
            }
            
            worksheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{prefix}_{DateTime.Now:yyyyMMddHHmm}.xlsx");
        }
    }

    private async Task<List<ColdFishWMS.Models.DTOs.ReportImportDetailDTO>> GetImportData(DateTime? startDate, DateTime? endDate)
    {
        var from = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to = endDate ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

        return await _context.ChiTietPhieuNhaps
            .Include(x => x.PhieuNhap).ThenInclude(p => p.NhaCungCap)
            .Include(x => x.SanPham)
            .Where(x => x.PhieuNhap.NgayNhap >= from && x.PhieuNhap.NgayNhap <= to)
            .OrderByDescending(x => x.PhieuNhap.NgayNhap)
            .Select(x => new ColdFishWMS.Models.DTOs.ReportImportDetailDTO
            {
                MaPhieu = x.MaPhieuNhap,
                NgayNhap = x.PhieuNhap.NgayNhap,
                NhaCungCap = x.PhieuNhap.NhaCungCap.TenNhaCungCap,
                MaSanPham = x.MaSanPham,
                TenSanPham = x.SanPham.TenSanPham,
                MaLoHang = x.MaLoHang,
                SoLuong = x.SoLuong,
                DonGia = x.DonGia,
                ThanhTien = x.SoLuong * x.DonGia
            }).ToListAsync();
    }

    private async Task<List<ColdFishWMS.Models.DTOs.ReportExportDetailDTO>> GetExportData(DateTime? startDate, DateTime? endDate)
    {
        var from = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var to = endDate ?? DateTime.Now.Date.AddDays(1).AddTicks(-1);

        return await _context.ChiTietPhieuXuats
            .Include(x => x.PhieuXuat).ThenInclude(p => p.KhachHang)
            .Include(x => x.SanPham)
            .Where(x => x.PhieuXuat.NgayXuat >= from && x.PhieuXuat.NgayXuat <= to)
            .OrderByDescending(x => x.PhieuXuat.NgayXuat)
            .Select(x => new ColdFishWMS.Models.DTOs.ReportExportDetailDTO
            {
                MaPhieu = x.MaPhieuXuat,
                NgayXuat = x.PhieuXuat.NgayXuat,
                KhachHang = x.PhieuXuat.KhachHang.TenKhachHang,
                MaSanPham = x.MaSanPham,
                TenSanPham = x.SanPham.TenSanPham,
                MaLoHang = x.MaLoHang,
                SoLuong = x.SoLuong,
                DonGia = x.DonGia,
                ThanhTien = x.SoLuong * x.DonGia
            }).ToListAsync();
    }

    private async Task<List<ColdFishWMS.Models.DTOs.ReportInventoryDetailDTO>> GetInventoryData()
    {
        return await _context.LoHangs
            .Include(x => x.SanPham)
            .Include(x => x.ViTriKho)
            .Include(x => x.ChiTietPhieuNhaps).ThenInclude(ct => ct.PhieuNhap)
            .Where(x => x.SoLuongTon > 0)
            .Select(x => new ColdFishWMS.Models.DTOs.ReportInventoryDetailDTO
            {
                MaLo = x.MaLoHang,
                MaSanPham = x.MaSanPham,
                TenSanPham = x.SanPham.TenSanPham,
                ViTri = x.ViTriKho.MaViTri,
                NgaySanXuat = x.NgaySanXuat,
                NgayNhap = x.ChiTietPhieuNhaps.OrderBy(ct => ct.PhieuNhap.NgayNhap).Select(ct => ct.PhieuNhap.NgayNhap).FirstOrDefault(),
                HanSuDung = x.HanSuDung,
                SoLuongTon = x.SoLuongTon,
                TrangThai = x.HanSuDung < DateTime.Today ? "Hết hạn" : (x.HanSuDung <= DateTime.Today.AddDays(30) ? "Sắp hết hạn" : "Bình thường")
            }).ToListAsync();
    }
    private (DateTime, DateTime) CalculateDateRange(string period, DateTime? startDate, DateTime? endDate)
    {
        DateTime from = DateTime.Now.Date;
        DateTime to = DateTime.Now.Date.AddDays(1).AddTicks(-1);

        switch (period?.ToLower())
        {
            case "today":
                from = DateTime.Today;
                to = DateTime.Today.AddDays(1).AddTicks(-1);
                break;
            case "week":
                int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
                from = DateTime.Today.AddDays(-1 * diff).Date;
                break;
            case "year":
                from = new DateTime(DateTime.Today.Year, 1, 1);
                break;
            case "month":
            default:
                if (!startDate.HasValue) from = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                else from = startDate.Value;
                break;
        }

        if (startDate.HasValue && period == "custom") from = startDate.Value;
        if (endDate.HasValue) to = endDate.Value;
        
        // Ensure 'to' covers end of day if not manually set to specific time
        if (to.Hour == 0 && to.Minute == 0 && to.Second == 0)
            to = to.Date.AddDays(1).AddTicks(-1);

        return (from, to);
    }
}
