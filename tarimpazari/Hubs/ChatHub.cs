using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TarimPazari.Business.Abstract;

namespace tarimpazari.Hubs
{
    /// <summary>
    /// SignalR Hub – Gerçek zamanlı sohbet mesajlaşması.
    /// Her conversation için ayrı bir grup oluşturulur.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IWebHostEnvironment _env;

        public ChatHub(IChatService chatService, IWebHostEnvironment env)
        {
            _chatService = chatService;
            _env = env;
        }

        /// <summary>
        /// Kullanıcıyı belirtilen sohbet grubuna ekler.
        /// </summary>
        public async Task JoinConversation(int conversationId)
        {
            var groupName = $"conversation_{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Kullanıcıyı sohbet grubundan çıkarır.
        /// </summary>
        public async Task LeaveConversation(int conversationId)
        {
            var groupName = $"conversation_{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Mesaj gönderir ve gruptaki diğer kullanıcılara iletir.
        /// </summary>
        public async Task SendMessage(int conversationId, string content)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var (success, message) = await _chatService.SendMessageAsync(conversationId, userId, content);
            if (success)
            {
                var groupName = $"conversation_{conversationId}";
                await Clients.Group(groupName).SendAsync("ReceiveMessage", new
                {
                    senderId = userId,
                    senderName = Context.User?.Identity?.Name ?? "Bilinmeyen",
                    content = content.Trim(),
                    sentAt = DateTime.Now.ToString("HH:mm")
                });
            }
        }

        /// <summary>
        /// Sesli mesaj gönderir: ses dosyası diske kaydedilir ve URL olarak iletilir.
        /// </summary>
        public async Task SendVoiceMessage(int conversationId, string audioBase64, int durationSeconds)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId)) return;

            var (success, message, audioUrl) = await _chatService.SendVoiceMessageAsync(
                conversationId, userId, audioBase64, durationSeconds, _env.WebRootPath);

            if (success && audioUrl != null)
            {
                var groupName = $"conversation_{conversationId}";
                await Clients.Group(groupName).SendAsync("ReceiveVoiceMessage", new
                {
                    senderId = userId,
                    senderName = Context.User?.Identity?.Name ?? "Bilinmeyen",
                    audioUrl = audioUrl,
                    durationSeconds = durationSeconds,
                    sentAt = DateTime.Now.ToString("HH:mm")
                });
            }
        }

        /// <summary>
        /// Yeni teklif bildirimini sohbet grubuna iletir.
        /// </summary>
        public async Task NotifyNewOffer(int conversationId, decimal offeredPrice, int quantity)
        {
            var groupName = $"conversation_{conversationId}";
            await Clients.Group(groupName).SendAsync("ReceiveOffer", new
            {
                senderId = Context.UserIdentifier,
                offeredPrice,
                quantity,
                sentAt = DateTime.Now.ToString("HH:mm")
            });
        }
    }
}
