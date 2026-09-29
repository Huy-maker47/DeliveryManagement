using DeliveryManagement.Data;
using DeliveryManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PriceTableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PriceTableController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách bảng giá
        public async Task<IActionResult> Index()
        {
            var list = await _context.PriceTables
                .OrderBy(p => p.DeliveryMethod)
                .ThenBy(p => p.MinWeight)
                .ToListAsync();

            return View(list);
        }

        // Thêm
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new PriceTable { DeliveryMethod = "Thường" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PriceTable model)
        {
            if (!await ValidateAsync(model))
            {
                return View("Form", model);
            }

            model.PriceTableId = 0;
            Normalize(model);

            _context.PriceTables.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm mức giá.";
            return RedirectToAction(nameof(Index));
        }

        // Sửa
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var entity = await _context.PriceTables.FindAsync(id);
            if (entity == null) return NotFound();

            return View("Form", entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PriceTable model)
        {
            if (!await ValidateAsync(model))
            {
                return View("Form", model);
            }

            var entity = await _context.PriceTables.FindAsync(model.PriceTableId);
            if (entity == null) return NotFound();

            Normalize(model);

            entity.DeliveryMethod = model.DeliveryMethod;
            entity.MinWeight = model.MinWeight;
            entity.MaxWeight = model.MaxWeight;
            entity.WeightRange = model.WeightRange;
            entity.DistanceRange = model.DistanceRange;
            entity.Price = model.Price;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật mức giá.";
            return RedirectToAction(nameof(Index));
        }

        // Xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.PriceTables.FindAsync(id);

            if (entity != null)
            {
                _context.PriceTables.Remove(entity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa mức giá.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // HỖ TRỢ
        // =========================

        private async Task<bool> ValidateAsync(PriceTable m)
        {
            // Hai trường này do server tự điền
            ModelState.Remove(nameof(PriceTable.WeightRange));
            ModelState.Remove(nameof(PriceTable.DistanceRange));

            if (m.MaxWeight <= m.MinWeight)
            {
                ModelState.AddModelError(
                    nameof(PriceTable.MaxWeight),
                    "Khối lượng tối đa phải lớn hơn khối lượng tối thiểu.");
            }
            else
            {
                var overlap = await _context.PriceTables.AnyAsync(p =>
                    p.PriceTableId != m.PriceTableId &&
                    p.DeliveryMethod == m.DeliveryMethod &&
                    p.MinWeight < m.MaxWeight &&
                    m.MinWeight < p.MaxWeight);

                if (overlap)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Khoảng khối lượng bị trùng với một mức giá khác của cùng hình thức giao hàng.");
                }
            }

            return ModelState.IsValid;
        }

        private static void Normalize(PriceTable m)
        {
            m.WeightRange = $"{m.MinWeight:0.##} - {m.MaxWeight:0.##} kg";
            m.DistanceRange = m.DistanceRange?.Trim() ?? string.Empty;
        }
    }
}
