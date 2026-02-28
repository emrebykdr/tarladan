using Microsoft.EntityFrameworkCore;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;
using TarimPazari.DataAccess.Repositories;

namespace TarimPazari.Business.Concrete
{
    /// <summary>
    /// Teklif/pazarlık iş kuralları servisi.
    /// Sohbet içinden teklif verme, kabul etme, reddetme işlemleri.
    /// Teklif kabul edildiğinde otomatik sipariş oluşturur.
    /// </summary>
    public class OfferService : IOfferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Offer>> GetOffersByConversationAsync(int conversationId)
        {
            return await _unitOfWork.GetRepository<Offer>()
                .Query()
                .Include(o => o.Sender)
                .Where(o => o.ConversationId == conversationId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Offer?> GetOfferByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Offer>()
                .Query()
                .Include(o => o.Conversation)
                    .ThenInclude(c => c!.Product)
                .Include(o => o.Sender)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// <summary>
        /// İş Kuralı: Teklif oluşturma validasyonları
        /// </summary>
        public async Task<(bool Success, string Message)> CreateOfferAsync(Offer offer)
        {
            // Kural 1: Sohbet var mı ve aktif mi?
            var conversation = await _unitOfWork.GetRepository<Conversation>()
                .Query()
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == offer.ConversationId);

            if (conversation == null || !conversation.IsActive)
                return (false, "Sohbet bulunamadı veya aktif değil.");

            // Kural 2: Gönderen bu sohbetin bir tarafı mı?
            if (conversation.BuyerId != offer.SenderId && conversation.SellerId != offer.SenderId)
                return (false, "Bu sohbette teklif verme yetkiniz yok.");

            // Kural 3: Ürün aktif mi?
            if (conversation.Product == null || !conversation.Product.IsActive)
                return (false, "Ürün artık aktif değil.");

            // Kural 4: Fiyat > 0
            if (offer.OfferedPrice <= 0)
                return (false, "Teklif fiyatı 0'dan büyük olmalıdır.");

            // Kural 5: Miktar >= minimum sipariş miktarı
            if (offer.Quantity < conversation.Product.MinimumOrderQuantity)
                return (false, $"Miktar minimum sipariş miktarından ({conversation.Product.MinimumOrderQuantity} {conversation.Product.Unit}) az olamaz.");

            // Kural 6: Miktar <= stok
            if (offer.Quantity > conversation.Product.StockQuantity)
                return (false, $"Stokta yeterli miktar yok. Mevcut stok: {conversation.Product.StockQuantity} {conversation.Product.Unit}.");

            // Kural 7: Bekleyen teklif varsa yeni teklif verilemez (aynı sohbette aynı kişiden)
            var pendingExists = await _unitOfWork.GetRepository<Offer>()
                .AnyAsync(o => o.ConversationId == offer.ConversationId &&
                              o.SenderId == offer.SenderId &&
                              o.Status == OfferStatus.Pending);
            if (pendingExists)
                return (false, "Zaten bekleyen bir teklifiniz var. Önce onu iptal edin veya yanıt bekleyin.");

            offer.Status = OfferStatus.Pending;
            offer.CreatedAt = DateTime.Now;

            await _unitOfWork.GetRepository<Offer>().AddAsync(offer);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Teklif gönderildi.");
        }

        /// <summary>
        /// İş Kuralı: Teklif kabul – otomatik Order oluşturur
        /// </summary>
        public async Task<(bool Success, string Message, int? OrderId)> AcceptOfferAsync(int offerId, string accepterId)
        {
            var offer = await _unitOfWork.GetRepository<Offer>()
                .Query()
                .Include(o => o.Conversation)
                    .ThenInclude(c => c!.Product)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return (false, "Teklif bulunamadı.", null);

            if (offer.Status != OfferStatus.Pending)
                return (false, "Sadece beklemede olan teklifler kabul edilebilir.", null);

            // Kabul eden, teklifin karşı tarafı olmalı
            var conversation = offer.Conversation!;
            if (offer.SenderId == accepterId)
                return (false, "Kendi teklifinizi kabul edemezsiniz.", null);

            if (conversation.BuyerId != accepterId && conversation.SellerId != accepterId)
                return (false, "Bu teklifi kabul etme yetkiniz yok.", null);

            // Teklifi kabul et
            offer.Status = OfferStatus.Accepted;
            offer.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Offer>().Update(offer);

            // Aynı sohbetteki diğer bekleyen teklifleri reddet
            var pendingOffers = await _unitOfWork.GetRepository<Offer>()
                .GetAllAsync(o => o.ConversationId == offer.ConversationId &&
                                  o.Id != offerId &&
                                  o.Status == OfferStatus.Pending);
            foreach (var pending in pendingOffers)
            {
                pending.Status = OfferStatus.Rejected;
                pending.UpdatedAt = DateTime.Now;
                _unitOfWork.GetRepository<Offer>().Update(pending);
            }

            // Sipariş oluştur
            var order = new Order
            {
                ConversationId = conversation.Id,
                OfferId = offerId,
                ProductId = conversation.ProductId,
                BuyerId = conversation.BuyerId,
                SellerId = conversation.SellerId,
                AgreedPrice = offer.OfferedPrice,
                Quantity = offer.Quantity,
                TotalAmount = offer.OfferedPrice * offer.Quantity,
                Status = OrderStatus.Agreed,
                DeliveryDate = offer.DeliveryDate,
                DeliveryLocation = offer.DeliveryLocation,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<Order>().AddAsync(order);

            // Stoktan düş
            var product = conversation.Product!;
            product.StockQuantity -= offer.Quantity;
            if (product.StockQuantity <= 0)
            {
                product.StockQuantity = 0;
                product.IsActive = false;
            }
            product.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Product>().Update(product);

            await _unitOfWork.SaveChangesAsync();

            return (true, "Teklif kabul edildi ve sipariş oluşturuldu!", order.Id);
        }

        /// <summary>
        /// İş Kuralı: Teklif reddetme
        /// </summary>
        public async Task<(bool Success, string Message)> RejectOfferAsync(int offerId, string rejecterId)
        {
            var offer = await _unitOfWork.GetRepository<Offer>()
                .Query()
                .Include(o => o.Conversation)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null)
                return (false, "Teklif bulunamadı.");

            if (offer.Status != OfferStatus.Pending)
                return (false, "Sadece beklemede olan teklifler reddedilebilir.");

            var conversation = offer.Conversation!;
            if (offer.SenderId == rejecterId)
                return (false, "Kendi teklifinizi reddedemezsiniz. İptal etmek istiyorsanız yeni teklif verin.");

            if (conversation.BuyerId != rejecterId && conversation.SellerId != rejecterId)
                return (false, "Bu teklifi reddetme yetkiniz yok.");

            offer.Status = OfferStatus.Rejected;
            offer.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Offer>().Update(offer);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Teklif reddedildi.");
        }

        /// <summary>
        /// Çiftçiye gelen bekleyen teklif sayısı (Dashboard için)
        /// </summary>
        public async Task<int> GetPendingOfferCountForSellerAsync(string sellerId)
        {
            return await _unitOfWork.GetRepository<Offer>()
                .Query()
                .Include(o => o.Conversation)
                .CountAsync(o => o.Conversation!.SellerId == sellerId &&
                                 o.Status == OfferStatus.Pending);
        }

        /// <summary>
        /// Alıcının toplam teklif sayısı (Dashboard için)
        /// </summary>
        public async Task<int> GetOfferCountByBuyerAsync(string buyerId)
        {
            return await _unitOfWork.GetRepository<Offer>()
                .Query()
                .CountAsync(o => o.SenderId == buyerId);
        }
    }
}
