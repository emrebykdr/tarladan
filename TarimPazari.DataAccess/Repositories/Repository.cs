using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TarimPazari.Core.Repositories;
using TarimPazari.DataAccess.Context;

namespace TarimPazari.DataAccess.Repositories
{
    /// <summary>
    /// Generic Repository – Core katmanındaki IRepository sözleşmesinin somut implementasyonu.
    /// Tüm Entity'ler için tek bir sınıf üzerinden CRUD işlemleri yapılır.
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        // Tümünü getir
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // Koşula göre filtreli getir
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        // Id ile tekil getir
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        // Koşula göre tekil getir
        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // IQueryable erişimi – Include (Eager Loading) için
        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        // Ekle
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        // Güncelle
        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        // Sil
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        // Varlık kontrolü
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Kayıt sayısı
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }
    }
}
