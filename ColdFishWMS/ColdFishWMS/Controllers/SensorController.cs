using ColdFishWMS.Data;
using ColdFishWMS.Models.DTOs;
using ColdFishWMS.Models.Entities;
using ColdFishWMS.Business.Services; // Assuming AlertConfigService is here or similar
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColdFishWMS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly ColdFishDbContext _context;
    private readonly IAlertConfigService _configService;
    private readonly IEmailService _emailService;

    public SensorController(ColdFishDbContext context, IAlertConfigService configService, IEmailService emailService)
    {
        _context = context;
        _configService = configService;
        _emailService = emailService;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateSensorData([FromBody] SensorDataDto data)
    {
        if (data == null) return BadRequest("Invalid data");

        // 1. Save Log
        var log = new NhatKyNhietDo
        {
            MaThietBi = data.DeviceId,
            NhietDo = data.Temperature,
            DoAm = data.Humidity,
            ThoiGianGhi = DateTime.Now
        };
        
        _context.NhatKyNhietDos.Add(log);
        await _context.SaveChangesAsync();

        // 2. Check Alerts
        await CheckTemperatureAlert(data);

        return Ok(new { message = "Data received" });
    }

    private async Task CheckTemperatureAlert(SensorDataDto data)
    {
        var config = await _configService.GetConfigAsync();
        
        bool isHigh = data.Temperature > config.MaxTemperature;
        bool isLow = data.Temperature < config.MinTemperature;

        if (isHigh || isLow)
        {
            var existing = await _context.CanhBaos
                .AnyAsync(c => c.LoaiCanhBao == "Nhiệt độ" 
                          && c.MaSanPham == null 
                          && !c.DaXuLy 
                          && c.NoiDung.Contains(data.DeviceId));

            if (!existing)
            {
                var msg = isHigh 
                    ? $"Nhiệt độ cao bất thường tại {data.DeviceId}: {data.Temperature}°C (Max: {config.MaxTemperature}°C)"
                    : $"Nhiệt độ thấp bất thường tại {data.DeviceId}: {data.Temperature}°C (Min: {config.MinTemperature}°C)";

                var alert = new CanhBao
                {
                    LoaiCanhBao = "Nhiệt độ",
                    MucDo = "Nguy hiểm",
                    NoiDung = msg,
                    NgayTao = DateTime.Now,
                };

                _context.CanhBaos.Add(alert);
                await _context.SaveChangesAsync();

                // 3. Send Email to Managers
                if (alert.MucDo == "Nguy hiểm")
                {
                   await SendEmailToManagers(alert);
                }
            }
        }
    }

    private async Task SendEmailToManagers(CanhBao alert)
    {
        // Get all managers with email
        var managers = await _context.NguoiDungs
            .Include(u => u.VaiTro)
            .Where(u => u.VaiTro.TenVaiTro == ColdFishWMS.Models.AppRoles.QuanLyKho && !string.IsNullOrEmpty(u.Email))
            .ToListAsync();

        foreach (var manager in managers)
        {
            await _emailService.SendEmailAsync(
                manager.Email!, 
                $"[ColdFishWMS] CẢNH BÁO NHIỆT ĐỘ: {alert.MucDo}", 
                $"<h3>Phát hiện cảnh báo mới</h3><p>{alert.NoiDung}</p><p>Thời gian: {alert.NgayTao}</p>");
        }
    }
}
