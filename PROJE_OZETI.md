# 🌾 TARLADAN — Tarım Pazarı Projesi

> **"Tarladan Sofrana, Aracısız Tarım Ticareti"**

## 📌 Projenin Amacı

**TARLADAN**, çiftçiler ile alıcılar arasında **aracısız tarım ticareti** sağlayan bir web uygulamasıdır. Çiftçiler ürünlerini doğrudan listeleyebilir, alıcılar ürünleri inceleyip fiyat pazarlığı yapabilir ve güvenli bir teslimat doğrulama sistemiyle alışverişi tamamlayabilir.

### Temel Hedefler
- Çiftçilerin ürünlerini aracısız satabilmesi
- Alıcıların taze tarım ürünlerine doğrudan ulaşması
- Gerçek zamanlı sohbet ve pazarlık imkânı
- Güvenli teslimat doğrulama (handshake protokolü)
- Konum tabanlı ürün bilgilendirmesi

---

## 🏗️ Mimari Yapı

### Teknoloji Yığını

| Katman | Teknoloji |
|--------|-----------|
| **Framework** | ASP.NET Core 9.0 (MVC) |
| **Veritabanı** | SQL Server + Entity Framework Core 9.0 |
| **Kimlik Doğrulama** | ASP.NET Core Identity |
| **Gerçek Zamanlı İletişim** | SignalR |
| **Frontend** | Razor Views + Vanilla CSS + Font Awesome |
| **Font** | Google Fonts (Inter) |
| **Dil** | C# 12, HTML5, JavaScript |

### Katmanlı Mimari (N-Tier Architecture)

```
┌─────────────────────────────────────────────────────────┐
│                    SUNUM KATMANI                        │
│        tarimpazari (Web MVC Projesi)                    │
│  Controllers │ Views │ ViewModels │ Hubs │ wwwroot      │
├─────────────────────────────────────────────────────────┤
│                    İŞ KATMANI                           │
│          TarimPazari.Business                           │
│        Abstract (Interfaces) │ Concrete (Services)      │
├─────────────────────────────────────────────────────────┤
│                  VERİ ERİŞİM KATMANI                    │
│          TarimPazari.DataAccess                         │
│        Context │ Repositories │ Migrations              │
├─────────────────────────────────────────────────────────┤
│                    ÇEKİRDEK KATMAN                      │
│           TarimPazari.Core                              │
│      Entities │ Enums │ Repositories (Interfaces)       │
└─────────────────────────────────────────────────────────┘
```

### Tasarım Desenleri

| Desen | Açıklama |
|-------|----------|
| **Repository Pattern** | `IRepository<T>` / `Repository<T>` — Generic CRUD işlemleri |
| **Unit of Work** | `IUnitOfWork` / `UnitOfWork` — İşlem bütünlüğü (transaction) |
| **Dependency Injection** | Tüm servislerin `Program.cs`'de DI container'a kaydı |
| **MVC Pattern** | Model-View-Controller ayrımı |
| **Soft Delete** | `IsDeleted` flag'i ile kayıtların mantıksal silinmesi |

---

## 👥 Rol ve Yetki Sistemi

| Rol | Yetkiler |
|-----|----------|
| **Admin** | Kullanıcı yönetimi (askıya alma, banlama, ürün yetkisi), tüm ürünleri görme/düzenleme |
| **Çiftçi (Ciftci)** | Ürün ekleme/düzenleme/silme, gelen teklifleri yönetme, sipariş durumu güncelleme, teslimat doğrulama |
| **Alıcı (Alici)** | Ürünleri inceleme, teklif verme (Bid + Offer), sohbet başlatma, sipariş takibi |

### Hesap Güvenliği
- **Şifre kuralları:** Min 6 karakter, büyük/küçük harf, rakam zorunlu
- **Hesap kilitleme:** 5 başarısız giriş → 5 dakika kilitlenme
- **Askıya alma (Suspend):** Admin tarafından geçici erişim engeli
- **Banlama (Ban):** Kalıcı erişim engeli
- **Ürün yetkisi:** Admin tarafından çiftçinin ürün ekleme yetkisi açılıp kapatılabilir

---

## 📁 Proje Dosya Yapısı

```
tarimpazari/
├── tarimpazari.slnx                    # Solution dosyası
│
├── TarimPazari.Core/                   # Çekirdek Katman
│   ├── Entities/                       # Veritabanı modelleri
│   │   ├── BaseEntity.cs              # Ortak alanlar (Id, CreatedAt, UpdatedAt, IsDeleted)
│   │   ├── ApplicationUser.cs         # Kullanıcı modeli (Identity genişletme)
│   │   ├── Product.cs                 # Ürün modeli
│   │   ├── Bid.cs                     # Teklif modeli (eski sistem)
│   │   ├── Conversation.cs            # Sohbet oturumu
│   │   ├── ChatMessage.cs             # Sohbet mesajı (sesli mesaj desteği dahil)
│   │   ├── Offer.cs                   # Sohbet içi teklif modeli (yeni sistem)
│   │   ├── Order.cs                   # Sipariş modeli
│   │   └── DeliveryCode.cs            # Teslimat doğrulama kodu
│   ├── Enums/                         # Sabit değer listeleri
│   │   ├── UserRoleType.cs            # Admin, Ciftci, Alici
│   │   ├── BidStatus.cs               # Beklemede, KabulEdildi, ReddEdildi, IptalEdildi, SuresiDoldu
│   │   ├── OfferStatus.cs             # Pending, Accepted, Rejected, Countered
│   │   └── OrderStatus.cs             # Negotiating, Agreed, Preparing, ReadyForPickup, Completed
│   └── Repositories/                  # Repository arayüzleri
│       ├── IRepository.cs             # Generic CRUD sözleşmesi
│       └── IUnitOfWork.cs             # İşlem bütünlüğü sözleşmesi
│
├── TarimPazari.DataAccess/             # Veri Erişim Katmanı
│   ├── Context/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext (Fluent API yapılandırması)
│   ├── Repositories/
│   │   ├── Repository.cs             # Generic Repository implementasyonu
│   │   └── UnitOfWork.cs             # UnitOfWork implementasyonu
│   └── Migrations/                    # Veritabanı migration dosyaları
│
├── TarimPazari.Business/              # İş Katmanı
│   ├── Abstract/                      # Servis arayüzleri
│   │   ├── IProductService.cs
│   │   ├── IBidService.cs
│   │   ├── IChatService.cs
│   │   ├── IOfferService.cs
│   │   └── IOrderService.cs
│   └── Concrete/                      # Servis implementasyonları
│       ├── ProductService.cs          # Ürün CRUD + arama + filtreleme
│       ├── BidService.cs              # Eski teklif sistemi
│       ├── ChatService.cs             # Sohbet + sesli mesaj
│       ├── OfferService.cs            # Sohbet içi teklif + sipariş oluşturma
│       └── OrderService.cs            # Sipariş yönetimi + teslimat doğrulama
│
├── tarimpazari/                        # Web (Sunum) Katmanı
│   ├── Program.cs                     # Uygulama başlangıç yapılandırması
│   ├── Controllers/
│   │   ├── HomeController.cs          # Ana sayfa + Dashboard + Arama
│   │   ├── AccountController.cs       # Kayıt / Giriş / Çıkış
│   │   ├── AdminController.cs         # Kullanıcı yönetimi
│   │   ├── ProductController.cs       # Ürün CRUD
│   │   ├── BidController.cs           # Eski teklif sistemi
│   │   ├── ChatController.cs          # Sohbet + sohbet içi teklif
│   │   └── OrderController.cs         # Sipariş detay + durum güncelleme
│   ├── Hubs/
│   │   └── ChatHub.cs                 # SignalR Hub (mesaj + sesli mesaj + teklif)
│   ├── ViewModels/
│   │   └── ViewModels.cs              # Tüm ViewModel'ler
│   ├── Views/
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml         # Ana şablon (navbar, kategori barı, footer)
│   │   │   └── Error.cshtml
│   │   ├── Home/
│   │   │   ├── Index.cshtml           # Pazar yeri (ürün listeleme)
│   │   │   ├── Dashboard.cshtml       # Kullanıcı paneli
│   │   │   └── About.cshtml           # Hakkımızda
│   │   ├── Account/
│   │   │   ├── Register.cshtml        # Kayıt formu
│   │   │   ├── Login.cshtml           # Giriş formu
│   │   │   └── AccessDenied.cshtml
│   │   ├── Product/
│   │   │   ├── Create.cshtml          # Ürün ekleme formu
│   │   │   ├── Edit.cshtml            # Ürün düzenleme
│   │   │   └── Details.cshtml         # Ürün detay sayfası
│   │   ├── Chat/
│   │   │   ├── Index.cshtml           # Sohbet listesi
│   │   │   └── Conversation.cshtml    # Sohbet sayfası (mesajlar + teklifler)
│   │   ├── Bid/
│   │   │   ├── Index.cshtml           # Alıcının teklifleri
│   │   │   ├── Create.cshtml          # Teklif verme formu
│   │   │   └── Incoming.cshtml        # Çiftçiye gelen teklifler
│   │   ├── Order/
│   │   │   ├── Index.cshtml           # Sipariş listesi
│   │   │   └── Details.cshtml         # Sipariş detay + teslimat doğrulama
│   │   └── Admin/
│   │       └── Index.cshtml           # Admin kullanıcı yönetim paneli
│   └── wwwroot/
│       ├── css/site.css               # Ana stil dosyası
│       └── uploads/voice/             # Sesli mesaj dosyaları (.webm)
```

---

## ⚙️ Çalıştığı Sistemler ve Modüller

### 1. 🏪 Pazar Yeri (Ürün Yönetimi)
- Çiftçiler ürün ekleyebilir (ad, açıklama, fiyat, birim, stok, kategori, konum, organik bilgisi, hasat tarihi, raf ömrü)
- Ürünler ana sayfada kart görünümünde listelenir
- **Kategori filtreleme:** Sebze, Meyve, Süt Ürünleri, Organik, Tahıl, Baklagil
- **Arama:** İsme göre arama
- Ürün detay sayfası

### 2. 💬 Gerçek Zamanlı Sohbet (SignalR)
- Alıcı, ürün detay sayfasından "Satıcıyla İletişime Geç" ile sohbet başlatır
- **Metin mesajları:** Gerçek zamanlı gönderim/alım
- **Sesli mesajlar:** Tarayıcı mikrofonu ile kayıt → base64 → sunucuya .webm olarak kayıt → URL ile paylaşım
- **Hazır cevaplar:** Alıcı ve satıcıya özel hızlı yanıt butonları
- **Okunmadı sayacı:** Okunmamış mesaj takibi

### 3. 🤝 Teklif ve Pazarlık Sistemi

#### Eski Sistem (Bid)
- Alıcı doğrudan ürün sayfasından teklif verir
- Çiftçi kabul/red yapabilir
- Durum akışı: Beklemede → KabulEdildi / ReddEdildi / IptalEdildi

#### Yeni Sistem (Offer — Sohbet İçi)
- Sohbet içinden birim fiyat, miktar, teslimat tarihi, buluşma yeri ve not ile teklif
- Kabul edildiğinde otomatik sipariş oluşturulur
- Durum akışı: Pending → Accepted / Rejected / Countered

### 4. 📦 Sipariş Yönetimi
Teklif kabul edildikten sonra sipariş oluşur:

```
Agreed (Anlaşıldı)
  → Preparing (Hazırlanıyor) — Bu aşamada DeliveryCode üretilir
    → ReadyForPickup (Teslimata Hazır)
      → Completed (Tamamlandı) — DeliveryCode doğrulaması ile
```

### 5. 🔐 Güvenli Teslimat Doğrulama (Handshake Protokolü)
1. Sipariş **Preparing** durumuna geldiğinde 6 haneli alfanümerik kod üretilir
2. Kod **alıcıya** gösterilir (7 gün geçerli)
3. Buluşmada alıcı kodu satıcıya söyler
4. Satıcı kodu sisteme girer → doğrulanırsa sipariş **Completed** olur

### 6. 🛡️ Admin Yönetim Paneli
- Tüm kullanıcıları listeleme (rol, şehir, durum, yetki bilgisi)
- Kullanıcı askıya alma / askıdan çıkarma (sebep belirtme)
- Kalıcı banlama / ban kaldırma
- Ürün ekleme yetkisi açma/kapama
- Kullanıcı silme (soft delete)

### 7. 📊 Kullanıcı Dashboard'u
- Rol bazlı özelleştirilmiş panel
- **Çiftçi:** Toplam ürün, bekleyen teklif sayısı, aktif sipariş, okunmadı mesaj
- **Alıcı:** Pazar ürün sayısı, teklif sayısı, aktif sipariş, okunmadı mesaj
- **Admin:** Toplam ürün, toplam kullanıcı sayısı

---

## 🗃️ Veritabanı Modelleri (Entity Relationship)

```
ApplicationUser (IdentityUser)
  ├── Products[]           ← Çiftçinin ürünleri
  ├── Bids[]               ← Alıcının teklifleri
  ├── BuyerOrders[]         ← Alıcı siparişleri
  └── SellerOrders[]        ← Satıcı siparişleri

Product
  ├── Seller               → ApplicationUser
  ├── Bids[]               → Bid
  └── Conversations[]      → Conversation

Conversation
  ├── Product              → Product
  ├── Buyer                → ApplicationUser
  ├── Seller               → ApplicationUser
  ├── Messages[]           → ChatMessage
  ├── Offers[]             → Offer
  └── Orders[]             → Order

ChatMessage
  ├── Conversation         → Conversation
  ├── Sender               → ApplicationUser
  └── AudioFilePath?       (sesli mesaj dosya yolu)

Offer
  ├── Conversation         → Conversation
  └── Sender               → ApplicationUser

Order
  ├── Conversation         → Conversation
  ├── Offer                → Offer
  ├── Product              → Product
  ├── Buyer                → ApplicationUser
  ├── Seller               → ApplicationUser
  └── DeliveryCode?        → DeliveryCode

DeliveryCode
  └── Order                → Order
```

---

## 🔧 Yapılandırma ve Bağımlılıklar

### Program.cs Yapılandırması
- **Veritabanı:** SQL Server bağlantısı (`DefaultConnection`)
- **Identity:** Şifre kuralları, kilit ayarları, e-posta benzersizliği
- **Cookie:** Login/Logout/AccessDenied yolları, 24 saat oturum süresi
- **DI Kayıtları:** Repository, UnitOfWork, 5 servis
- **SignalR:** Max mesaj boyutu 512 KB (sesli mesajlar için)
- **Middleware:** HTTPS yönlendirme, Authentication, Authorization, Static Files
- **Seed Data:** Admin hesabı otomatik oluşturma, 3 rol (Admin, Ciftci, Alici)

### NuGet Paketleri
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.SignalR`

### Frontend Bağımlılıkları (CDN)
- Font Awesome 6.5 (ikonlar)
- Google Fonts — Inter (tipografi)
- SignalR JavaScript Client 8.0

---

## 🚀 Nasıl Çalıştırılır

```bash
# 1. Proje dizinine git
cd tarimpazari/tarimpazari

# 2. Veritabanını güncelle
dotnet ef database update --project ../TarimPazari.DataAccess

# 3. Uygulamayı başlat
dotnet run
```

Uygulama **http://localhost:5149** adresinde çalışır.

### Varsayılan Hesaplar

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | admin@tarimpazari.com | Admin123! |
| Alıcı | mehmet@test.com | Test1234 |
| Çiftçi | ahmet@test.com | Test1234 |

---

## 📈 İstatistikler

| Metrik | Değer |
|--------|-------|
| Toplam Proje | 4 (Core, DataAccess, Business, Web) |
| Entity Sayısı | 9 |
| Controller Sayısı | 7 |
| Servis Sayısı | 5 |
| View Klasörü | 8 |
| Enum Sayısı | 4 |
| SignalR Hub | 1 |
| ViewModel | 8 |
| Migration Dosyası | Çoklu |
