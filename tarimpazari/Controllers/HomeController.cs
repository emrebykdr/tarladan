using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using tarimpazari.ViewModels;

namespace tarimpazari.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IChatService _chatService;
        private readonly IOfferService _offerService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IProductService productService,
            IChatService chatService,
            IOfferService offerService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _chatService = chatService;
            _offerService = offerService;
            _orderService = orderService;
            _userManager = userManager;
        }

        // Ana sayfa – Pazar Yeri
        public async Task<IActionResult> Index(string? search, string? category)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _productService.SearchProductsAsync(search);
                ViewBag.SearchTerm = search;
            }
            else if (!string.IsNullOrEmpty(category))
            {
                products = await _productService.GetProductsByCategoryAsync(category);
                ViewBag.SelectedCategory = category;
            }
            else
            {
                products = await _productService.GetActiveProductsAsync();
            }

            return View(products);
        }

        // Kullanıcı Paneli
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var userId = user.Id;
            var roles = await _userManager.GetRolesAsync(user);

            var model = new DashboardViewModel
            {
                FullName = user.FullName,
                UserRole = roles.FirstOrDefault() ?? "Bilinmiyor",
                UnreadMessages = await _chatService.GetUnreadCountAsync(userId),
                ActiveOrders = await _orderService.GetActiveOrderCountAsync(userId)
            };

            if (roles.Contains("Ciftci"))
            {
                var products = await _productService.GetProductsBySellerAsync(userId);
                model.TotalProducts = products.Count();
                model.PendingOffers = await _offerService.GetPendingOfferCountForSellerAsync(userId);
            }
            else if (roles.Contains("Alici"))
            {
                var products = await _productService.GetActiveProductsAsync();
                model.TotalProducts = products.Count();
                model.TotalOffers = await _offerService.GetOfferCountByBuyerAsync(userId);
            }
            else if (roles.Contains("Admin"))
            {
                model.TotalProducts = await _productService.GetProductCountAsync();
                model.TotalUsers = _userManager.Users.Count();
            }

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
