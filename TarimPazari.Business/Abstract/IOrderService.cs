using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;

namespace TarimPazari.Business.Abstract
{
    /// <summary>
    /// Sipariş yönetimi ve teslimat doğrulama için servis sözleşmesi.
    /// </summary>
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string userId);
        Task<(bool Success, string Message, string? Code)> GenerateDeliveryCodeAsync(int orderId);
        Task<(bool Success, string Message)> VerifyDeliveryCodeAsync(int orderId, string code, string sellerId);
        Task<int> GetActiveOrderCountAsync(string userId);
    }
}
