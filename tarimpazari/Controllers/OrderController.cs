using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;
using tarimpazari.ViewModels;

namespace tarimpazari.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Order - Siparişlerim
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            ViewBag.CurrentUserId = userId;
            return View(orders);
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null) return NotFound();
            if (order.BuyerId != userId && order.SellerId != userId) return Forbid();

            ViewBag.CurrentUserId = userId;
            ViewBag.IsSeller = order.SellerId == userId;
            return View(order);
        }

        // POST: /Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ciftci")]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message) = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, userId);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Details", new { id = orderId });
        }

        // POST: /Order/VerifyDelivery
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ciftci")]
        public async Task<IActionResult> VerifyDelivery(int orderId, string code)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message) = await _orderService.VerifyDeliveryCodeAsync(orderId, code, userId);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
