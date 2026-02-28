using System.ComponentModel.DataAnnotations;
using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;

namespace tarimpazari.ViewModels
{
    // ========== HESAP (ACCOUNT) ViewModel'leri ==========
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Şifre Tekrar")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        [Display(Name = "Hesap Türü")]
        public UserRoleType RoleType { get; set; }

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [Display(Name = "İlçe")]
        public string? District { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        // Çiftçi için ek alanlar
        [Display(Name = "Çiftlik Adı")]
        public string? FarmName { get; set; }

        [Display(Name = "Çiftlik Konumu")]
        public string? FarmLocation { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }

    // ========== ÜRÜN ViewModel'leri ==========
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [MaxLength(200)]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Birim Fiyat (₺)")]
        public decimal Price { get; set; }

        [Display(Name = "Birim")]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok negatif olamaz.")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Minimum sipariş en az 1 olmalıdır.")]
        [Display(Name = "Minimum Sipariş Miktarı")]
        public int MinimumOrderQuantity { get; set; } = 1;

        [Display(Name = "Kategori")]
        public string? Category { get; set; }

        [Display(Name = "Görsel URL")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Menşe İli")]
        public string? CityOfOrigin { get; set; }

        [Display(Name = "Organik mi?")]
        public bool IsOrganic { get; set; }

        [Display(Name = "Sertifika Detayları")]
        public string? CertificationDetails { get; set; }

        [Display(Name = "Hasat Tarihi")]
        [DataType(DataType.Date)]
        public DateTime HarvestDate { get; set; } = DateTime.Now;

        [Display(Name = "Tahmini Raf Ömrü (Gün)")]
        public int EstimatedShelfLifeDays { get; set; }

        // --- KONUM BİLGİLERİ (Yeni) ---
        [Required(ErrorMessage = "Enlem zorunludur.")]
        [Display(Name = "Enlem")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Boylam zorunludur.")]
        [Display(Name = "Boylam")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Konum adresi zorunludur.")]
        [MaxLength(500)]
        [Display(Name = "Konum Adresi")]
        public string LocationAddress { get; set; } = string.Empty;
    }

    // ========== SOHBET & TEKLİF ViewModel'leri (Yeni) ==========
    public class ChatViewModel
    {
        public Conversation Conversation { get; set; } = null!;
        public List<ChatMessage> Messages { get; set; } = new();
        public List<Offer> Offers { get; set; } = new();
        public string CurrentUserId { get; set; } = string.Empty;
        public bool IsSeller { get; set; }
    }

    public class OfferCreateViewModel
    {
        public int ConversationId { get; set; }

        [Required(ErrorMessage = "Teklif fiyatı zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Teklif Fiyatı (₺/birim)")]
        public decimal OfferedPrice { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        [Display(Name = "Miktar")]
        public int Quantity { get; set; }

        [Display(Name = "Teslimat Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DeliveryDate { get; set; }

        [Display(Name = "Teslimat/Buluşma Yeri")]
        [MaxLength(500)]
        public string? DeliveryLocation { get; set; }

        [Display(Name = "Not")]
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    // ========== HIZLI TEKLİF ViewModel ==========
    public class QuickOfferViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Teklif fiyatı zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Teklif Fiyatı (₺/birim)")]
        public decimal OfferedPrice { get; set; }

        [Required(ErrorMessage = "Miktar zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
        [Display(Name = "Miktar")]
        public int Quantity { get; set; }
    }

    // ========== DASHBOARD ViewModel ==========
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOffers { get; set; }
        public int PendingOffers { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveOrders { get; set; }
        public int UnreadMessages { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
