namespace ColdFishWMS.Models.DTOs;

public class SensorDataDto
{
    public string DeviceId { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double? Humidity { get; set; }
    public string? Token { get; set; }
}
