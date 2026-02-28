using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimPazari.Core.Entities
{
    /// <summary>
    /// Güvenli teslimat doğrulama kodu.
    /// Sipariş "Preparing" aşamasına geçtiğinde otomatik üretilir.
    /// Satıcı, alıcıdan bu kodu alıp sisteme girdiğinde sipariş tamamlanır.
    /// </summary>
    public class DeliveryCode : BaseEntity
    {
        // İlişkili sipariş
        public int OrderId { get; set; }

        // 6 haneli alfanümerik doğrulama kodu
        [Required]
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        // Kodun geçerlilik süresi
        public DateTime ExpiresAt { get; set; }

        // Kod kullanıldı mı
        public bool IsUsed { get; set; } = false;

        // Kullanılma zamanı
        public DateTime? UsedAt { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
