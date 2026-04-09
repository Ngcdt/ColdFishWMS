using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ColdFishWMS.Models;
using ColdFishWMS.Business.Services;
using ColdFishWMS.Models.Entities;

namespace ColdFishWMS.Controllers;

[Authorize(Roles = AppRoles.QuanLyKho + "," + AppRoles.NhanVienKho + "," + AppRoles.KeToanKho)]
public class ViTriKhoController : Controller
{
    private readonly IViTriKhoService _service;

    public ViTriKhoController(IViTriKhoService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _service.GetAllAsync();
        return View(list);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ViTriKho model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _service.AddAsync(model);
                TempData["Success"] = "Thêm vị trí mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
        }
        return View(model);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var entity = await _service.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ViTriKho model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Cập nhật vị trí thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
        }
        return View(model);
    }

    [Authorize(Roles = AppRoles.QuanLyKho)]
    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            // Check dependency before delete if needed, assume Service/Repo handles or DB FK handles restriction
            var entity = await _service.GetByIdAsync(id);
            if(entity != null && entity.LoHangs != null && entity.LoHangs.Any())
            {
                 TempData["Error"] = "Không thể xóa vị trí đang chứa hàng!";
                 return RedirectToAction(nameof(Index));
            }

            await _service.DeleteAsync(id);
            TempData["Success"] = "Xóa vị trí thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
