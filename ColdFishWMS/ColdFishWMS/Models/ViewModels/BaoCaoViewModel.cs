using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Models.ViewModels;

public class BaoCaoViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    
    // Chart Data (6 Months)
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> ChartDataImport { get; set; } = new();
    public List<decimal> ChartDataExport { get; set; } = new();

    // New Charts
    public List<string> PieChartLabels { get; set; } = new();
    public List<decimal> PieChartData { get; set; } = new();

    public List<string> BarChartLabels { get; set; } = new();
    public List<decimal> BarChartData { get; set; } = new();

    // FEFO Stats
    public int FefoPercentage { get; set; } = 100;
    public int FefoCorrect { get; set; }
    public int FefoViolation { get; set; }
    public int SpoilageCount { get; set; }

    // Detailed List for Table
    public List<ReportDetailItem> ReportDetails { get; set; } = new();
}

public class ReportDetailItem
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; }
    public string BatchCode { get; set; }
    public string Type { get; set; } // "Nhập" / "Xuất"
    public decimal Quantity { get; set; }
    public string ClassType { get; set; } // badge-success / badge-primary
}
