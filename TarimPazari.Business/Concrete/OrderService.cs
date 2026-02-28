using Microsoft.EntityFrameworkCore;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;
using TarimPazari.DataAccess.Repositories;

namespace TarimPazari.Business.Concrete
{
    /// <summary>
    /// Sipariş yönetimi ve güvenli teslimat doğrulama servisi.
    /// Durum akışı: Agreed → Preparing → ReadyForPickup → Completed
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private static readonly Random _random = new();

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
        {
            return await _unitOfWork.GetRepository<Order>()
                .Query()
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.DeliveryCode)
                .Where(o => o.BuyerId == userId || o.SellerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Order>()
                .Query()
                .Include(o => o.Product)
                .Include(o => o.Buyer)
                .Include(o => o.Seller)
                .Include(o => o.DeliveryCode)
                .Include(o => o.Conversation)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// <summary>
        /// İş Kuralı: Sipariş durumu güncelleme – sıralı geçiş zorunlu.
        /// Sadece Çiftçi (satıcı) durumu ilerletebilir.
        /// Preparing aşamasına geçişte DeliveryCode üretilir.
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string userId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
                return (false, "Sipariş bulunamadı.");

            // Sadece satıcı durumu güncelleyebilir
            if (order.SellerId != userId)
                return (false, "Sadece satıcı sipariş durumunu güncelleyebilir.");

            // Durum geçişi sıralı olmalı
            var expectedPrevious = newStatus switch
            {
                OrderStatus.Preparing => OrderStatus.Agreed,
                OrderStatus.ReadyForPickup => OrderStatus.Preparing,
                _ => (OrderStatus?)null
            };

            if (expectedPrevious == null)
                return (false, "Geçersiz durum geçişi. Yalnızca Preparing veya ReadyForPickup durumlarına geçiş yapılabilir.");

            if (order.Status != expectedPrevious)
                return (false, $"Sipariş şu anda '{GetStatusText(order.Status)}' durumunda. Doğrudan '{GetStatusText(newStatus)}' durumuna geçilemez.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Order>().Update(order);

            // Preparing aşamasına geçiş → DeliveryCode üret
            if (newStatus == OrderStatus.Preparing)
            {
                var (success, message, code) = await GenerateDeliveryCodeAsync(orderId);
                if (!success)
                    return (false, message);
            }

            await _unitOfWork.SaveChangesAsync();
            return (true, $"Sipariş durumu '{GetStatusText(newStatus)}' olarak güncellendi.");
        }

        /// <summary>
        /// 6 haneli alfanümerik teslimat doğrulama kodu üretir.
        /// </summary>
        public async Task<(bool Success, string Message, string? Code)> GenerateDeliveryCodeAsync(int orderId)
        {
            // Zaten kod var mı kontrol et
            var existingCode = await _unitOfWork.GetRepository<DeliveryCode>()
                .GetAsync(d => d.OrderId == orderId && !d.IsUsed);
            if (existingCode != null)
                return (true, "Kod zaten mevcut.", existingCode.Code);

            // 6 haneli alfanümerik kod üret
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Karışıklığı önlemek için I, O, 0, 1 hariç
            var code = new string(Enumerable.Range(0, 6)
                .Select(_ => chars[_random.Next(chars.Length)])
                .ToArray());

            var deliveryCode = new DeliveryCode
            {
                OrderId = orderId,
                Code = code,
                ExpiresAt = DateTime.Now.AddDays(7), // 7 gün geçerli
                IsUsed = false,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<DeliveryCode>().AddAsync(deliveryCode);
            // SaveChanges dışarıda yapılır (UpdateOrderStatus içinden)

            return (true, "Teslimat doğrulama kodu üretildi.", code);
        }

        /// <summary>
        /// İş Kuralı: Teslimat kodu doğrulama – Handshake Protokolü
        /// Satıcı, alıcıdan aldığı kodu sisteme girer.
        /// Kod doğruysa sipariş otomatik Completed olur.
        /// </summary>
        public async Task<(bool Success, string Message)> VerifyDeliveryCodeAsync(int orderId, string code, string sellerId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
                return (false, "Sipariş bulunamadı.");

            // Sadece satıcı doğrulama yapabilir
            if (order.SellerId != sellerId)
                return (false, "Sadece satıcı teslimat doğrulaması yapabilir.");

            // Sipariş ReadyForPickup durumunda olmalı
            if (order.Status != OrderStatus.ReadyForPickup)
                return (false, "Sipariş henüz teslimata hazır değil.");

            // Teslimat kodu kontrolü
            var deliveryCode = order.DeliveryCode;
            if (deliveryCode == null)
                return (false, "Teslimat doğrulama kodu bulunamadı.");

            if (deliveryCode.IsUsed)
                return (false, "Bu kod zaten kullanılmış.");

            if (deliveryCode.ExpiresAt < DateTime.Now)
                return (false, "Doğrulama kodunun süresi dolmuş.");

            if (deliveryCode.Code != code.ToUpper().Trim())
                return (false, "Doğrulama kodu hatalı. Lütfen tekrar deneyin.");

            // ✅ Doğrulama başarılı – Siparişi tamamla
            deliveryCode.IsUsed = true;
            deliveryCode.UsedAt = DateTime.Now;
            _unitOfWork.GetRepository<DeliveryCode>().Update(deliveryCode);

            order.Status = OrderStatus.Completed;
            order.CompletedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Order>().Update(order);

            await _unitOfWork.SaveChangesAsync();

            return (true, "✅ Teslimat doğrulandı! Sipariş başarıyla tamamlandı.");
        }

        public async Task<int> GetActiveOrderCountAsync(string userId)
        {
            return await _unitOfWork.GetRepository<Order>()
                .Query()
                .CountAsync(o => (o.BuyerId == userId || o.SellerId == userId) &&
                                  o.Status != OrderStatus.Completed);
        }

        private static string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Negotiating => "Pazarlık Aşamasında",
                OrderStatus.Agreed => "Anlaşıldı",
                OrderStatus.Preparing => "Hazırlanıyor",
                OrderStatus.ReadyForPickup => "Teslimata Hazır",
                OrderStatus.Completed => "Tamamlandı",
                _ => status.ToString()
            };
        }
    }
}
