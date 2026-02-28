using System.Linq.Expressions;

namespace TarimPazari.Core.Repositories
{
    /// <summary>
    /// Generic Repository Interface – Tüm CRUD işlemleri için tek bir sözleşme.
    /// Data Access katmanında somutlaştırılacaktır.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // Tümünü getir
        Task<IEnumerable<T>> GetAllAsync();

        // Koşula göre filtreli getir
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

        // Id ile tekil getir
        Task<T?> GetByIdAsync(int id);

        // Koşula göre tekil getir
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

        // IQueryable erişimi – Include (Eager Loading) için
        IQueryable<T> Query();

        // Ekle
        Task AddAsync(T entity);

        // Güncelle
        void Update(T entity);

        // Sil (Fiziksel)
        void Delete(T entity);

        // Varlık kontrolü
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

        // Kayıt sayısı
        Task<int> CountAsync();
    }
}
