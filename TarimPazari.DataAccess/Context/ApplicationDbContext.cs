using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TarimPazari.Core.Entities;

namespace TarimPazari.DataAccess.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet Tanımları
        public DbSet<Product> Products { get; set; }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DeliveryCode> DeliveryCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity tablolarının oluşması için şart
            base.OnModelCreating(modelBuilder);

            // ========== PRODUCT KONFIGÜRASYONU ==========
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);

                // Fiyat alanları için hassasiyet (para birimi)
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                // Soft Delete filtresi – Silinen ürünler sorguya dahil edilmez
                entity.HasQueryFilter(p => !p.IsDeleted);

                // Seller (Çiftçi) ile ilişki
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Index'ler – Performans için
                entity.HasIndex(p => p.Category);
                entity.HasIndex(p => p.SellerId);
                entity.HasIndex(p => p.CityOfOrigin);
            });



            // ========== CONVERSATION (SOHBET) KONFIGÜRASYONU ==========
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasQueryFilter(c => !c.IsDeleted);

                entity.HasOne(c => c.Product)
                      .WithMany(p => p.Conversations)
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Buyer)
                      .WithMany()
                      .HasForeignKey(c => c.BuyerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Seller)
                      .WithMany()
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => c.ProductId);
                entity.HasIndex(c => c.BuyerId);
                entity.HasIndex(c => c.SellerId);
            });

            // ========== CHAT MESSAGE KONFIGÜRASYONU ==========
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasQueryFilter(m => !m.IsDeleted);

                entity.HasOne(m => m.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => m.ConversationId);
                entity.HasIndex(m => m.SentAt);
            });

            // ========== OFFER (TEKLİF/PAZARLIK) KONFIGÜRASYONU ==========
            modelBuilder.Entity<Offer>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.OfferedPrice)
                      .HasColumnType("decimal(18,2)");

                entity.HasQueryFilter(o => !o.IsDeleted);

                entity.HasOne(o => o.Conversation)
                      .WithMany(c => c.Offers)
                      .HasForeignKey(o => o.ConversationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Sender)
                      .WithMany()
                      .HasForeignKey(o => o.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(o => o.ConversationId);
                entity.HasIndex(o => o.Status);
            });

            // ========== ORDER (SİPARİŞ) KONFIGÜRASYONU ==========
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.AgreedPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                entity.HasQueryFilter(o => !o.IsDeleted);

                entity.HasOne(o => o.Conversation)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.ConversationId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Offer)
                      .WithMany()
                      .HasForeignKey(o => o.OfferId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Product)
                      .WithMany()
                      .HasForeignKey(o => o.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Buyer)
                      .WithMany(u => u.BuyerOrders)
                      .HasForeignKey(o => o.BuyerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Seller)
                      .WithMany(u => u.SellerOrders)
                      .HasForeignKey(o => o.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(o => o.Status);
                entity.HasIndex(o => o.BuyerId);
                entity.HasIndex(o => o.SellerId);
                entity.HasIndex(o => o.ProductId);
            });

            // ========== DELIVERY CODE KONFIGÜRASYONU ==========
            modelBuilder.Entity<DeliveryCode>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.HasOne(d => d.Order)
                      .WithOne(o => o.DeliveryCode)
                      .HasForeignKey<DeliveryCode>(d => d.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(d => d.OrderId).IsUnique();
                entity.HasIndex(d => d.Code);
            });

            // ========== APPLICATION USER KONFIGÜRASYONU ==========
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.TrustScore)
                      .HasDefaultValue(5.0);

                entity.HasIndex(u => u.City);
            });
        }
    }
}
