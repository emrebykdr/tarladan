using Microsoft.AspNetCore.Identity;
using TarimPazari.Core.Enums;

namespace TarimPazari.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRoleType RoleType { get; set; }

        // Çiftçi (Farmer) alanları
        public string? FarmName { get; set; }
        public string? FarmLocation { get; set; }
        public double TrustScore { get; set; } = 5.0;
        public string? ProfileImageUrl { get; set; }

        // Ortak alanlar
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public bool IsApproved { get; set; } = true;       // Hesap otomatik aktif
        public bool IsSuspended { get; set; } = false;      // Askıya alınma durumu
        public string? SuspendReason { get; set; }          // Askıya alma sebebi
        public bool IsBanned { get; set; } = false;         // Kalıcı ban
        public string? BanReason { get; set; }              // Ban sebebi
        public bool CanAddProducts { get; set; } = true;    // Ürün ekleme yetkisi
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<Product>? Products { get; set; }
        public virtual ICollection<Order>? BuyerOrders { get; set; }
        public virtual ICollection<Order>? SellerOrders { get; set; }
    }
}
