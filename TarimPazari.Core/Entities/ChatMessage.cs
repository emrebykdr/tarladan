using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimPazari.Core.Entities
{
    /// <summary>
    /// Sohbet içindeki bireysel mesajlar.
    /// </summary>
    public class ChatMessage : BaseEntity
    {
        // Hangi sohbete ait
        public int ConversationId { get; set; }

        // Mesajı gönderen kullanıcı
        [Required]
        public string SenderId { get; set; } = string.Empty;

        // Mesaj içeriği
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        // Okundu bilgisi
        public bool IsRead { get; set; } = false;

        // Gönderilme zamanı
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Sesli mesaj dosya yolu (varsa)
        [MaxLength(500)]
        public string? AudioFilePath { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }
    }
}
