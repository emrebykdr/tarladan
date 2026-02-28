using Microsoft.EntityFrameworkCore;
using TarimPazari.Business.Abstract;
using TarimPazari.Core.Entities;
using TarimPazari.DataAccess.Repositories;

namespace TarimPazari.Business.Concrete
{
    /// <summary>
    /// Ürün iş kuralları servisi.
    /// Veri tabanına yazılmadan önce tüm mantıksal denetimler burada yapılır.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Include(p => p.Seller)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Include(p => p.Seller)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySellerAsync(string sellerId)
        {
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Include(p => p.Seller)
                .Where(p => p.Category == category && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Include(p => p.Seller)
                .Where(p => p.IsActive &&
                    (p.Name.ToLower().Contains(term) ||
                     (p.Description != null && p.Description.ToLower().Contains(term)) ||
                     (p.Category != null && p.Category.ToLower().Contains(term)) ||
                     (p.CityOfOrigin != null && p.CityOfOrigin.ToLower().Contains(term)) ||
                     (p.LocationAddress.ToLower().Contains(term))))
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _unitOfWork.GetRepository<Product>()
                .Query()
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// İş Kuralı: Ürün ekleme validasyonları
        /// </summary>
        public async Task<(bool Success, string Message)> AddProductAsync(Product product)
        {
            // Kural 1: Ürün adı boş olamaz
            if (string.IsNullOrWhiteSpace(product.Name))
                return (false, "Ürün adı boş bırakılamaz.");

            // Kural 2: Fiyat 0'dan büyük olmalı
            if (product.Price <= 0)
                return (false, "Ürün fiyatı 0'dan büyük olmalıdır.");

            // Kural 3: Stok miktarı negatif olamaz
            if (product.StockQuantity < 0)
                return (false, "Stok miktarı negatif olamaz.");

            // Kural 4: Minimum sipariş miktarı en az 1 olmalı
            if (product.MinimumOrderQuantity < 1)
                return (false, "Minimum sipariş miktarı en az 1 olmalıdır.");

            // Kural 5: Konum bilgileri zorunlu
            if (string.IsNullOrWhiteSpace(product.LocationAddress))
                return (false, "Konum adresi zorunludur.");

            // Kural 6: Aynı satıcıdan aynı isimde ürün olmamalı
            var exists = await _unitOfWork.GetRepository<Product>()
                .AnyAsync(p => p.SellerId == product.SellerId && p.Name == product.Name);
            if (exists)
                return (false, "Bu isimde bir ürününüz zaten mevcut.");

            product.CreatedAt = DateTime.Now;
            product.IsActive = true;

            await _unitOfWork.GetRepository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Ürün başarıyla eklendi.");
        }

        /// <summary>
        /// İş Kuralı: Ürün güncelleme validasyonları
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateProductAsync(Product product)
        {
            var existingProduct = await _unitOfWork.GetRepository<Product>().GetByIdAsync(product.Id);
            if (existingProduct == null)
                return (false, "Ürün bulunamadı.");

            if (product.Price <= 0)
                return (false, "Ürün fiyatı 0'dan büyük olmalıdır.");

            if (product.StockQuantity < 0)
                return (false, "Stok miktarı negatif olamaz.");

            if (string.IsNullOrWhiteSpace(product.LocationAddress))
                return (false, "Konum adresi zorunludur.");

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Unit = product.Unit;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.MinimumOrderQuantity = product.MinimumOrderQuantity;
            existingProduct.Category = product.Category;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.CityOfOrigin = product.CityOfOrigin;
            existingProduct.IsOrganic = product.IsOrganic;
            existingProduct.CertificationDetails = product.CertificationDetails;
            existingProduct.HarvestDate = product.HarvestDate;
            existingProduct.EstimatedShelfLifeDays = product.EstimatedShelfLifeDays;
            existingProduct.Latitude = product.Latitude;
            existingProduct.Longitude = product.Longitude;
            existingProduct.LocationAddress = product.LocationAddress;
            existingProduct.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Product>().Update(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Ürün başarıyla güncellendi.");
        }

        /// <summary>
        /// Soft Delete – Ürün fiziksel olarak silinmez, IsDeleted = true yapılır.
        /// Sadece ürünün sahibi silebilir.
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteProductAsync(int id, string sellerId)
        {
            var product = await _unitOfWork.GetRepository<Product>().GetByIdAsync(id);
            if (product == null)
                return (false, "Ürün bulunamadı.");

            if (product.SellerId != sellerId)
                return (false, "Yalnızca kendi ürününüzü silebilirsiniz.");

            product.IsDeleted = true;
            product.IsActive = false;
            product.UpdatedAt = DateTime.Now;

            _unitOfWork.GetRepository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Ürün başarıyla silindi.");
        }

        public async Task<int> GetProductCountAsync()
        {
            return await _unitOfWork.GetRepository<Product>().CountAsync();
        }
    }
}
