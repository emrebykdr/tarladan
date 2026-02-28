using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarimPazari.Core.Entities;

namespace tarimpazari.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var users = _userManager.Users
                .Where(u => !u.IsDeleted)
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            return View(users);
        }

        // POST: /Admin/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(string id, string? reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsSuspended = true;
            user.SuspendReason = reason;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} askıya alındı.";
            return RedirectToAction("Index");
        }

        // POST: /Admin/Unsuspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsuspend(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsSuspended = false;
            user.SuspendReason = null;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} askıdan çıkarıldı.";
            return RedirectToAction("Index");
        }

        // POST: /Admin/Ban
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ban(string id, string? reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = true;
            user.BanReason = reason;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} kalıcı olarak yasaklandı.";
            return RedirectToAction("Index");
        }

        // POST: /Admin/Unban
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unban(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = false;
            user.BanReason = null;
            user.IsSuspended = false;
            user.SuspendReason = null;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} yasağı kaldırıldı.";
            return RedirectToAction("Index");
        }

        // POST: /Admin/ToggleProducts — Ürün ekleme yetkisi aç/kapat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProducts(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.CanAddProducts = !user.CanAddProducts;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            var status = user.CanAddProducts ? "açıldı" : "kapatıldı";
            TempData["Success"] = $"{user.FullName} için ürün ekleme yetkisi {status}.";
            return RedirectToAction("Index");
        }

        // POST: /Admin/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.FullName} silindi.";
            return RedirectToAction("Index");
        }
    }
}
