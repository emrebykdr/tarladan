using TarimPazari.Core.Entities;

namespace TarimPazari.Business.Abstract
{
    /// <summary>
    /// Sohbet işlemleri için servis sözleşmesi.
    /// </summary>
    public interface IChatService
    {
        Task<Conversation?> GetConversationByIdAsync(int id);
        Task<IEnumerable<Conversation>> GetConversationsByUserAsync(string userId);
        Task<Conversation?> GetExistingConversationAsync(int productId, string buyerId);
        Task<(bool Success, string Message, int ConversationId)> StartConversationAsync(int productId, string buyerId);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId);
        Task<(bool Success, string Message)> SendMessageAsync(int conversationId, string senderId, string content);
        Task<(bool Success, string Message, string? AudioUrl)> SendVoiceMessageAsync(int conversationId, string senderId, string audioBase64, int durationSeconds, string webRootPath);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
