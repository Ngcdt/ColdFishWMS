using System.ComponentModel.DataAnnotations;

namespace ColdFishWMS.Models.ViewModels
{
    public class AlertConfigViewModel
    {
        [Display(Name = "Ngưỡng tồn kho tối thiểu")]
        public int MinStockThreshold { get; set; } = 10;

        [Display(Name = "Số ngày cảnh báo HSD")]
        public int ExpiryWarningDays { get; set; } = 7;

        [Display(Name = "Số ngày rủi ro hết hạn")]
        public int ExpiryDangerDays { get; set; } = 3;

        [Display(Name = "Ngưỡng hiệu suất xuất kho (%)")]
        public double ExportEfficiencyThreshold { get; set; } = 80;

        [Display(Name = "Chu kỳ luân chuyển lô hàng (ngày)")]
        public int BatchTurnoverCycle { get; set; } = 30;

        [Display(Name = "Nhiệt độ kho tối đa (°C)")]
        public double MaxTemperature { get; set; } = -18.0;

        [Display(Name = "Nhiệt độ kho tối thiểu (°C)")]
        public double MinTemperature { get; set; } = -25.0;

        [Display(Name = "Tỷ lệ cảnh báo chiếm dụng kho (%)")]
        public double WarehouseOccupancyThreshold { get; set; } = 85;
    }
}
