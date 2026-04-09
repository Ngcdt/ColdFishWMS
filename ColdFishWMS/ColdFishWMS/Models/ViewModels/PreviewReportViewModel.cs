using ColdFishWMS.Models.DTOs;

namespace ColdFishWMS.Models.ViewModels;

public class PreviewReportViewModel
{
    public string Title { get; set; }
    public string Type { get; set; } // Import, Export, Inventory
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public List<ReportImportDetailDTO> DataImport { get; set; }
    public List<ReportExportDetailDTO> DataExport { get; set; }
    public List<ReportInventoryDetailDTO> DataInventory { get; set; }

    // Chart Data for Preview
    public string ChartTitle { get; set; }
    public List<string> ChartLabels { get; set; } = new();
    public List<decimal> ChartData { get; set; } = new();
}
