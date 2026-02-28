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
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IOfferService _offerService;
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(IChatService chatService, IOfferService offerService, IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _offerService = offerService;
            _productService = productService;
            _userManager = userManager;
        }

        // GET: /Chat - Konuşma listesi
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var conversations = await _chatService.GetConversationsByUserAsync(userId);
            ViewBag.CurrentUserId = userId;
            return View(conversations);
        }

        // GET: /Chat/Conversation/5
        public async Task<IActionResult> Conversation(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var conversation = await _chatService.GetConversationByIdAsync(id);

            if (conversation == null)
                return NotFound();

            // Kullanıcı bu sohbetin tarafı mı?
            if (conversation.BuyerId != userId && conversation.SellerId != userId)
                return Forbid();

            var messages = await _chatService.GetMessagesAsync(id);
            var offers = await _offerService.GetOffersByConversationAsync(id);

            var model = new ChatViewModel
            {
                Conversation = conversation,
                Messages = messages.ToList(),
                Offers = offers.ToList(),
                CurrentUserId = userId,
                IsSeller = conversation.SellerId == userId
            };

            return View(model);
        }

        // POST: /Chat/Start/5 (ProductId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Alici")]
        public async Task<IActionResult> Start(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message, conversationId) = await _chatService.StartConversationAsync(id, userId);

            if (success)
                return RedirectToAction("Conversation", new { id = conversationId });

            TempData["Error"] = message;
            return RedirectToAction("Details", "Product", new { id });
        }

        // POST: /Chat/CreateOffer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffer(OfferCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lütfen tüm alanları doğru doldurunuz.";
                return RedirectToAction("Conversation", new { id = model.ConversationId });
            }

            var userId = _userManager.GetUserId(User)!;

            var offer = new Offer
            {
                ConversationId = model.ConversationId,
                SenderId = userId,
                OfferedPrice = model.OfferedPrice,
                Quantity = model.Quantity,
                DeliveryDate = model.DeliveryDate,
                DeliveryLocation = model.DeliveryLocation,
                Note = model.Note
            };

            var (success, message) = await _offerService.CreateOfferAsync(offer);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Conversation", new { id = model.ConversationId });
        }

        // POST: /Chat/AcceptOffer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOffer(int id, int conversationId)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message, orderId) = await _offerService.AcceptOfferAsync(id, userId);
            TempData[success ? "Success" : "Error"] = message;

            if (success && orderId.HasValue)
                return RedirectToAction("Details", "Order", new { id = orderId });

            return RedirectToAction("Conversation", new { id = conversationId });
        }

        // POST: /Chat/RejectOffer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectOffer(int id, int conversationId)
        {
            var userId = _userManager.GetUserId(User)!;
            var (success, message) = await _offerService.RejectOfferAsync(id, userId);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction("Conversation", new { id = conversationId });
        }

        // POST: /Chat/QuickOffer — Ürün detayından hızlı teklif (sohbet + teklif birlikte)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Alici")]
        public async Task<IActionResult> QuickOffer(QuickOfferViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lütfen fiyat ve miktar alanlarını doğru doldurunuz.";
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            var userId = _userManager.GetUserId(User)!;

            // 1. Sohbet başlat (veya mevcut sohbeti bul)
            var (chatSuccess, chatMessage, conversationId) = await _chatService.StartConversationAsync(model.ProductId, userId);
            if (!chatSuccess)
            {
                TempData["Error"] = chatMessage;
                return RedirectToAction("Details", "Product", new { id = model.ProductId });
            }

            // 2. Teklif oluştur
            var offer = new Offer
            {
                ConversationId = conversationId,
                SenderId = userId,
                OfferedPrice = model.OfferedPrice,
                Quantity = model.Quantity
            };

            var (offerSuccess, offerMessage) = await _offerService.CreateOfferAsync(offer);
            TempData[offerSuccess ? "Success" : "Error"] = offerMessage;

            // Sohbet sayfasına yönlendir
            return RedirectToAction("Conversation", new { id = conversationId });
        }
    }
}
