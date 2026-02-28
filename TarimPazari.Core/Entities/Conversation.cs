using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimPazari.Core.Entities
{
    /// <summary>
    /// Alıcı-Satıcı arasındaki sohbet oturumu.
    /// Bir ürün üzerinden başlatılır.
    /// </summary>
    public class Conversation : BaseEntity
    {
        // İlişkili ürün
        public int ProductId { get; set; }

        // Alıcı (sohbeti başlatan)
        [Required]
        public string BuyerId { get; set; } = string.Empty;

        // Satıcı (çiftçi / ürün sahibi)
        [Required]
        public string SellerId { get; set; } = string.Empty;

        // Sohbet aktif mi
        public bool IsActive { get; set; } = true;

        // --- Navigation Properties ---
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("BuyerId")]
        public virtual ApplicationUser? Buyer { get; set; }

        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }

        public virtual ICollection<ChatMessage>? Messages { get; set; }
        public virtual ICollection<Offer>? Offers { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}
