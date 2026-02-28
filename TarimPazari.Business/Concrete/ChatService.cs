using Microsoft.EntityFrameworkCore;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.DataAccess.Repositories;

namespace TarimPazari.Business.Concrete
{
    /// <summary>
    /// Sohbet iş kuralları servisi.
    /// Alıcı-Satıcı arasında sohbet başlatma, mesaj gönderme ve konuşma listeleme.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Conversation?> GetConversationByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Conversation>()
                .Query()
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Conversation>> GetConversationsByUserAsync(string userId)
        {
            return await _unitOfWork.GetRepository<Conversation>()
                .Query()
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages!.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.BuyerId == userId || c.SellerId == userId)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetExistingConversationAsync(int productId, string buyerId)
        {
            return await _unitOfWork.GetRepository<Conversation>()
                .GetAsync(c => c.ProductId == productId && c.BuyerId == buyerId && c.IsActive);
        }

        /// <summary>
        /// İş Kuralı: Sohbet başlatma validasyonları
        /// </summary>
        public async Task<(bool Success, string Message, int ConversationId)> StartConversationAsync(int productId, string buyerId)
        {
            // Kural 1: Ürün var mı ve aktif mi?
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(productId);
            if (product == null || !product.IsActive)
                return (false, "Ürün bulunamadı veya aktif değil.", 0);

            // Kural 2: Kendi ürününe sohbet başlatamaz
            if (product.SellerId == buyerId)
                return (false, "Kendi ürününüze sohbet başlatamazsınız.", 0);

            // Kural 3: Aynı alıcı-satıcı-ürün için aktif sohbet var mı?
            var existing = await GetExistingConversationAsync(productId, buyerId);
            if (existing != null)
                return (true, "Mevcut sohbet açıldı.", existing.Id);

            var conversation = new Conversation
            {
                ProductId = productId,
                BuyerId = buyerId,
                SellerId = product.SellerId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.GetRepository<Conversation>().AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Sohbet başlatıldı.", conversation.Id);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId)
        {
            return await _unitOfWork.GetRepository<ChatMessage>()
                .Query()
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        /// <summary>
        /// İş Kuralı: Mesaj gönderme validasyonları
        /// </summary>
        public async Task<(bool Success, string Message)> SendMessageAsync(int conversationId, string senderId, string content)
        {
            // Kural 1: Sohbet var mı ve aktif mi?
            var conversation = await _unitOfWork.GetRepository<Conversation>().GetByIdAsync(conversationId);
            if (conversation == null || !conversation.IsActive)
                return (false, "Sohbet bulunamadı veya aktif değil.");

            // Kural 2: Gönderen bu sohbetin bir tarafı mı?
            if (conversation.BuyerId != senderId && conversation.SellerId != senderId)
                return (false, "Bu sohbete mesaj gönderme yetkiniz yok.");

            // Kural 3: Mesaj boş olamaz
            if (string.IsNullOrWhiteSpace(content))
                return (false, "Mesaj boş olamaz.");

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim(),
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _unitOfWork.GetRepository<ChatMessage>().AddAsync(message);

            // Sohbetin son güncelleme tarihini ayarla
            conversation.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Conversation>().Update(conversation);

            await _unitOfWork.SaveChangesAsync();

            return (true, "Mesaj gönderildi.");
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _unitOfWork.GetRepository<ChatMessage>()
                .Query()
                .CountAsync(m => !m.IsRead &&
                    m.SenderId != userId &&
                    (m.Conversation!.BuyerId == userId || m.Conversation.SellerId == userId));
        }
        public async Task<(bool Success, string Message, string? AudioUrl)> SendVoiceMessageAsync(int conversationId, string senderId, string audioBase64, int durationSeconds, string webRootPath)
        {
            // Kural 1: Sohbet var mı ve aktif mi?
            var conversation = await _unitOfWork.GetRepository<Conversation>().GetByIdAsync(conversationId);
            if (conversation == null || !conversation.IsActive)
                return (false, "Sohbet bulunamadı veya aktif değil.", null);

            // Kural 2: Gönderen bu sohbetin bir tarafı mı?
            if (conversation.BuyerId != senderId && conversation.SellerId != senderId)
                return (false, "Bu sohbete mesaj gönderme yetkiniz yok.", null);

            // Ses dosyasını kaydet
            var uploadsDir = Path.Combine(webRootPath, "uploads", "voice");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}.webm";
            var filePath = Path.Combine(uploadsDir, fileName);
            var audioBytes = Convert.FromBase64String(audioBase64);
            await File.WriteAllBytesAsync(filePath, audioBytes);

            var audioUrl = $"/uploads/voice/{fileName}";

            var durationText = $"{durationSeconds / 60:D2}:{durationSeconds % 60:D2}";
            var content = $"🎤 Sesli mesaj ({durationText})";

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                AudioFilePath = audioUrl,
                SentAt = DateTime.Now,
                IsRead = false
            };

            await _unitOfWork.GetRepository<ChatMessage>().AddAsync(message);
            conversation.UpdatedAt = DateTime.Now;
            _unitOfWork.GetRepository<Conversation>().Update(conversation);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Sesli mesaj gönderildi.", audioUrl);
        }
    }
}
