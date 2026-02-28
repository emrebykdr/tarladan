using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;
using tarimpazari.ViewModels;

namespace tarimpazari.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                RoleType = model.RoleType,
                City = model.City,
                District = model.District,
                Address = model.Address,
                FarmName = model.FarmName,
                FarmLocation = model.FarmLocation,
                IsApproved = true // Hesap hemen aktif
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleName = model.RoleType.ToString();
                await _userManager.AddToRoleAsync(user, roleName);

                // Otomatik giriş yap
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Kayıt başarılı! Hoş geldiniz.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
                return View(model);
            }

            // Banlanmış kullanıcı kontrolü
            if (user.IsBanned)
            {
                var reason = !string.IsNullOrEmpty(user.BanReason) ? $" Sebep: {user.BanReason}" : "";
                ModelState.AddModelError(string.Empty, $"Hesabınız kalıcı olarak yasaklanmıştır.{reason}");
                return View(model);
            }

            // Askıya alınmış kullanıcı kontrolü
            if (user.IsSuspended)
            {
                var reason = !string.IsNullOrEmpty(user.SuspendReason) ? $" Sebep: {user.SuspendReason}" : "";
                ModelState.AddModelError(string.Empty, $"Hesabınız askıya alınmıştır.{reason}");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/");

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
