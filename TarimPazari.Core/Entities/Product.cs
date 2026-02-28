using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimPazari.Core.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public decimal Price { get; set; }                // Birim fiyat (TL/kg, TL/adet vb.)

        [MaxLength(50)]
        public string? Unit { get; set; }                 // Birim (kg, adet, ton, litre vb.)

        public int StockQuantity { get; set; }            // Mevcut stok miktarı
        public int MinimumOrderQuantity { get; set; } = 1; // Minimum sipariş miktarı

        [MaxLength(100)]
        public string? Category { get; set; }             // Ürün kategorisi (Meyve, Sebze, Tahıl vb.)

        public string? ImageUrl { get; set; }             // Ürün görseli
        
        [MaxLength(100)]
        public string? CityOfOrigin { get; set; }         // Ürünün yetiştirildiği il

        public bool IsActive { get; set; } = true;        // Ürün aktif mi?

        // --- KALİTE VE ÜRETİM DETAYLARI ---
        public bool IsOrganic { get; set; } = false;      // Ürün organik mi?
        
        [MaxLength(200)]
        public string? CertificationDetails { get; set; } // Varsa organik tarım veya iyi tarım sertifika numarası/detayı

        public DateTime HarvestDate { get; set; }         // Hasat tarihi (Tazelik ve fiyat belirleme için kritik)
        
        public int EstimatedShelfLifeDays { get; set; }   // Tahmini raf ömrü (Gıda israfını önlemek ve acil satışları öne çıkarmak için)


        // --- KONUM BİLGİLERİ ---
        [Required]
        public double Latitude { get; set; }              // Enlem

        [Required]
        public double Longitude { get; set; }             // Boylam

        [Required]
        [MaxLength(500)]
        public string LocationAddress { get; set; } = string.Empty;  // Açık adres


        // --- İLİŞKİLER (Navigation Properties) ---
        
        // Not: Identity varsayılan olarak string (Guid formatında metin) kullanır.
        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }   // Satıcı (Çiftçi)
        
        public virtual ICollection<Conversation>? Conversations { get; set; }  // Sohbetler
    }
}
