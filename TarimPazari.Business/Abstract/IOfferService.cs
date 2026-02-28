using TarimPazari.Core.Entities;
using TarimPazari.Core.Enums;

namespace TarimPazari.Business.Abstract
{
    /// <summary>
    /// Teklif/pazarlık işlemleri için servis sözleşmesi.
    /// </summary>
    public interface IOfferService
    {
        Task<IEnumerable<Offer>> GetOffersByConversationAsync(int conversationId);
        Task<Offer?> GetOfferByIdAsync(int id);
        Task<(bool Success, string Message)> CreateOfferAsync(Offer offer);
        Task<(bool Success, string Message, int? OrderId)> AcceptOfferAsync(int offerId, string accepterId);
        Task<(bool Success, string Message)> RejectOfferAsync(int offerId, string rejecterId);
        Task<int> GetPendingOfferCountForSellerAsync(string sellerId);
        Task<int> GetOfferCountByBuyerAsync(string buyerId);
    }
}
