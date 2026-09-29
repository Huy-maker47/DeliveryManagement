using DeliveryManagement.Data;
using DeliveryManagement.Models;
using DeliveryManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeliveryManagement.Controllers
{
    [Authorize(Roles = "Customer")]
    public class DeliveryOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeliveryOrderController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task LoadPackageTypesAsync()
        {
            ViewBag.PackageTypes = await _context.PackageTypes
                .Where(p => p.IsActive)
                .OrderBy(p => p.TypeName)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            var model = new CreateOrderViewModel
            {
                SenderName = user.FullName,
                SenderPhone = user.PhoneNumber ?? string.Empty,
                SenderAddress = user.DefaultSenderAddress ?? string.Empty,
                Items = new List<OrderItemInputModel> { new OrderItemInputModel() }
            };

            await LoadPackageTypesAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                await LoadPackageTypesAsync();
                return View(model);
            }
            var typeIds = model.Items
                .Select(i => i.PackageTypeId)
                .Distinct()
                .ToList();

            var validCount = await _context.PackageTypes
                .CountAsync(p => typeIds.Contains(p.PackageTypeId) && p.IsActive);

            if (validCount != typeIds.Count)
            {
                ModelState.AddModelError(
                    nameof(model.Items),
                    "Có loại đóng gói không hợp lệ.");

                await LoadPackageTypesAsync();
                return View(model);
            }
            string orderCode;

            do
            {
                orderCode = $"DH{DateTime.Now:yyyyMMddHHmmssfff}";
            }
            while (await _context.DeliveryOrders
                .AnyAsync(o => o.OrderCode == orderCode));
            var totalWeight = model.Items.Sum(i => i.Weight * i.Quantity);

            var price = await _context.PriceTables
                .Where(p =>
                    p.DeliveryMethod == model.DeliveryMethod &&
                    p.MinWeight < totalWeight &&
                    totalWeight <= p.MaxWeight)
                .OrderBy(p => p.MinWeight)
                .Select(p => (decimal?)p.Price)
                .FirstOrDefaultAsync();

            var order = new DeliveryOrder
            {
                OrderCode = orderCode,
                CustomerId = user.Id,

                SenderName = model.SenderName,
                SenderPhone = model.SenderPhone,
                SenderAddress = model.SenderAddress,

                ReceiverName = model.ReceiverName,
                ReceiverPhone = model.ReceiverPhone,
                ReceiverAddress = model.ReceiverAddress,

                DeliveryMethod = model.DeliveryMethod,

                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Chưa thanh toán",

                TotalAmount = price ?? 0,

                CurrentStatus = "Chưa nhận",
                CreatedAt = DateTime.Now
            };
            var orderItems = model.Items.Select(i => new OrderItem
            {
                OrderCode = orderCode,
                PackageTypeId = i.PackageTypeId,
                Description = i.Description,
                Size = i.Size,
                Quantity = i.Quantity,
                Weight = i.Weight,
                IsFragile = i.IsFragile,
                IsValuable = i.IsValuable
            }).ToList();
            var statusHistory = new OrderStatusHistory
            {
                OrderCode = orderCode,
                Status = "Chưa nhận",
                ChangedBy = user.Id,
                ChangedAt = DateTime.Now
            };

            _context.DeliveryOrders.Add(order);
            _context.OrderItems.AddRange(orderItems);
            _context.OrderStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            // Cập nhật thông tin người gửi
            user.DefaultSenderAddress = model.SenderAddress;

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                user.FullName = model.SenderName;
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                user.PhoneNumber = model.SenderPhone;
            }

            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = price == null
                ? $"Tạo yêu cầu giao hàng thành công. Mã đơn: {orderCode}. Chưa có mức giá phù hợp trong bảng giá nên tổng tiền chờ xác định."
                : $"Tạo yêu cầu giao hàng thành công. Mã đơn: {orderCode}";

            return RedirectToAction("Details", new { id = orderCode });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (string.IsNullOrWhiteSpace(id)) return NotFound();

            var order = await _context.DeliveryOrders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.PackageType)
                .FirstOrDefaultAsync(o => o.OrderCode == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy mã đơn hàng.";

                return RedirectToAction("Index", "Home");
            }

            var order = await _context.DeliveryOrders
                .FirstOrDefaultAsync(o =>
                    o.OrderCode == id &&
                    o.CustomerId == user.Id);

            if (order == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy đơn hàng hoặc bạn không có quyền hủy đơn này.";

                return RedirectToAction("Index", "Home");
            }
            if (order.CurrentStatus != "Chưa nhận")
            {
                TempData["ErrorMessage"] =
                    "Đơn hàng không còn ở trạng thái có thể hủy.";

                return RedirectToAction("Index", "Home");
            }
            order.CurrentStatus = "Đã hủy";
            var statusHistory = new OrderStatusHistory
            {
                OrderCode = order.OrderCode,
                Status = "Đã hủy",
                ChangedBy = user.Id,
                ChangedAt = DateTime.Now
            };

            _context.OrderStatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Đã hủy yêu cầu giao hàng {order.OrderCode}.";

            return RedirectToAction("Details", new { id = order.OrderCode });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string newStatus)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var order = await _context.DeliveryOrders.FirstOrDefaultAsync(o => o.OrderCode == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
            }
            string[] validStatuses = { "Chưa nhận", "Đã nhận - Chưa giao", "Đã nhận – Đang giao", "Đã Giao" };

            if (validStatuses.Contains(newStatus))
            {
                order.CurrentStatus = newStatus;
                var statusHistory = new OrderStatusHistory
                {
                    OrderCode = order.OrderCode,
                    Status = newStatus,
                    ChangedBy = user.Id,
                    ChangedAt = DateTime.Now
                };

                _context.DeliveryOrders.Update(order);
                _context.OrderStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn {order.OrderCode} thành: {newStatus}";
            }
            return RedirectToAction("Details", new { id = order.OrderCode });
        }
    }
}