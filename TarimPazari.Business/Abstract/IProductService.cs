using TarimPazari.Core.Entities;

namespace TarimPazari.Business.Abstract
{
    /// <summary>
    /// Ürün işlemleri için servis sözleşmesi.
    /// </summary>
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsBySellerAsync(string sellerId);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetProductByIdAsync(int id);
        Task<(bool Success, string Message)> AddProductAsync(Product product);
        Task<(bool Success, string Message)> UpdateProductAsync(Product product);
        Task<(bool Success, string Message)> DeleteProductAsync(int id, string sellerId);  // Soft Delete
        Task<int> GetProductCountAsync();
    }
}
