using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TarimPazari.Core.Enums;

namespace TarimPazari.Core.Entities
{
    /// <summary>
    /// Teklif kabul edildikten sonra oluşturulan sipariş.
    /// Durum akışı: Agreed → Preparing → ReadyForPickup → Completed
    /// </summary>
    public class Order : BaseEntity
    {
        // İlişkili sohbet
        public int ConversationId { get; set; }

        // Kabul edilen teklif
        public int OfferId { get; set; }

        // İlişkili ürün
        public int ProductId { get; set; }

        // Alıcı
        [Required]
        public string BuyerId { get; set; } = string.Empty;

        // Satıcı (çiftçi)
        [Required]
        public string SellerId { get; set; } = string.Empty;

        // Anlaşılan birim fiyat
        public decimal AgreedPrice { get; set; }

        // Miktar
        public int Quantity { get; set; }

        // Toplam tutar (AgreedPrice * Quantity)
        public decimal TotalAmount { get; set; }

        // Sipariş durumu
        public OrderStatus Status { get; set; } = OrderStatus.Agreed;

        // Teslimat tarihi
        public DateTime? DeliveryDate { get; set; }

        // Teslimat/buluşma yeri
        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        // Tamamlanma zamanı
        public DateTime? CompletedAt { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("ConversationId")]
        public virtual Conversation? Conversation { get; set; }

        [ForeignKey("OfferId")]
        public virtual Offer? Offer { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("BuyerId")]
        public virtual ApplicationUser? Buyer { get; set; }

        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }

        public virtual DeliveryCode? DeliveryCode { get; set; }
    }
}
