using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using tarimpazari.ViewModels;

namespace tarimpazari.Controllers
{
    [Authorize(Roles = "Ciftci,Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        // GET: /Product/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !user.CanAddProducts && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Ürün ekleme yetkiniz kısıtlanmıştır. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Dashboard", "Home");
            }
            return View(new ProductCreateViewModel());
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User)!;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !user.CanAddProducts && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Ürün ekleme yetkiniz kısıtlanmıştır.";
                return RedirectToAction("Dashboard", "Home");
            }
            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                Unit = model.Unit,
                StockQuantity = model.StockQuantity,
                MinimumOrderQuantity = model.MinimumOrderQuantity,
                Category = model.Category,
                ImageUrl = model.ImageUrl,
                CityOfOrigin = model.CityOfOrigin,
                IsOrganic = model.IsOrganic,
                CertificationDetails = model.CertificationDetails,
                HarvestDate = model.HarvestDate,
                EstimatedShelfLifeDays = model.EstimatedShelfLifeDays,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                LocationAddress = model.LocationAddress,
                SellerId = userId,
                IsActive = true
            };

            var (success, message) = await _productService.AddProductAsync(product);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = message;
            return View(model);
        }

        // GET: /Product/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var userId = _userManager.GetUserId(User)!;
            if (product.SellerId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var model = new ProductCreateViewModel
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Unit = product.Unit,
                StockQuantity = product.StockQuantity,
                MinimumOrderQuantity = product.MinimumOrderQuantity,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                CityOfOrigin = product.CityOfOrigin,
                IsOrganic = product.IsOrganic,
                CertificationDetails = product.CertificationDetails,
                HarvestDate = product.HarvestDate,
                EstimatedShelfLifeDays = product.EstimatedShelfLifeDays,
                Latitude = product.Latitude,
                Longitude = product.Longitude,
                LocationAddress = product.LocationAddress
            };

            ViewBag.ProductId = product.Id;
            return View(model);
        }

        // POST: /Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductId = id;
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var existingProduct = await _productService.GetProductByIdAsync(id);
            if (existingProduct == null) return NotFound();
            if (existingProduct.SellerId != userId && !User.IsInRole("Admin")) return Forbid();

            existingProduct.Name = model.Name;
            existingProduct.Description = model.Description;
            existingProduct.Price = model.Price;
            existingProduct.Unit = model.Unit;
            existingProduct.StockQuantity = model.StockQuantity;
            existingProduct.MinimumOrderQuantity = model.MinimumOrderQuantity;
            existingProduct.Category = model.Category;
            existingProduct.ImageUrl = model.ImageUrl;
            existingProduct.CityOfOrigin = model.CityOfOrigin;
            existingProduct.IsOrganic = model.IsOrganic;
            existingProduct.CertificationDetails = model.CertificationDetails;
            existingProduct.HarvestDate = model.HarvestDate;
            existingProduct.EstimatedShelfLifeDays = model.EstimatedShelfLifeDays;
            existingProduct.Latitude = model.Latitude;
            existingProduct.Longitude = model.Longitude;
            existingProduct.LocationAddress = model.LocationAddress;

            var (success, message) = await _productService.UpdateProductAsync(existingProduct);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction("Index", "Home");
            }

            TempData["Error"] = message;
            ViewBag.ProductId = id;
            return View(model);
        }

        // GET: /Product/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message) = await _productService.DeleteProductAsync(id, userId);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("Index", "Home");
        }
    }
}
