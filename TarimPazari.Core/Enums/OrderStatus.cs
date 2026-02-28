namespace TarimPazari.Core.Enums
{
    public enum OrderStatus
    {
        Negotiating = 1,     // Pazarlık Aşamasında
        Agreed = 2,          // Anlaşıldı
        Preparing = 3,       // Sipariş Hazırlanıyor
        ReadyForPickup = 4,  // Teslimat/Buluşma İçin Hazır
        Completed = 5        // Teslim Edildi
    }
}
