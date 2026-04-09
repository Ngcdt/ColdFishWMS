using System.Text.Json;
using ColdFishWMS.Models.ViewModels;

namespace ColdFishWMS.Business.Services
{
    public interface IAlertConfigService
    {
        Task<AlertConfigViewModel> GetConfigAsync();
        Task SaveConfigAsync(AlertConfigViewModel config);
    }

    public class AlertConfigService : IAlertConfigService
    {
        private readonly string _filePath;

        public AlertConfigService(IWebHostEnvironment env)
        {
            // Store config in ContentRoot/Data/alert_config.json
            _filePath = Path.Combine(env.ContentRootPath, "Data", "alert_config.json");
        }

        public async Task<AlertConfigViewModel> GetConfigAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new AlertConfigViewModel(); // Default
            }

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<AlertConfigViewModel>(json) ?? new AlertConfigViewModel();
            }
            catch
            {
                return new AlertConfigViewModel();
            }
        }

        public async Task SaveConfigAsync(AlertConfigViewModel config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
