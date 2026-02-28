using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TarimPazari.Core.Enums;

namespace TarimPazari.Core.Entities
{
    /// <summary>
    /// Sohbet içinden verilen fiyat/pazarlık teklifi.
    /// Alıcı veya satıcı tarafından gönderilebilir.
    /// </summary>
    public class Offer : BaseEntity
    {
        // Hangi sohbete ait
        public int ConversationId { get; set; }

        // Teklifi veren kullanıcı
        [Required]
        public string SenderId { get; set; } = string.Empty;

        // Teklif edilen birim fiyat
        public decimal OfferedPrice { get; set; }

        // Teklif edilen miktar
        public int Quantity { get; set; }

        // Teklif edilen teslimat tarihi
        public DateTime? DeliveryDate { get; set; }

        // Teslimat/buluşma yeri
        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        // Teklif notu
        [MaxLength(500)]
        public string? Note { get; set; }

        // Teklif durumu
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        // --- Navigation Properties ---
        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; }
    }
}
