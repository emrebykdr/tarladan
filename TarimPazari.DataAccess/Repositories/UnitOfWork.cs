using TarimPazari.Core.Repositories;
using TarimPazari.DataAccess.Context;

namespace TarimPazari.DataAccess.Repositories
{
    /// <summary>
    /// UnitOfWork – Birden fazla Repository üzerinde yapılan işlemleri
    /// tek bir SaveChanges() çağrısıyla veri tabanına yansıtır.
    /// Transaction bütünlüğünü sağlar.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : class;
        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// İstenen Entity tipi için Repository döndürür.
        /// Daha önce oluşturulduysa cache'den getirir.
        /// </summary>
        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repository = new Repository<T>(_context);
                _repositories[type] = repository;
            }

            return (IRepository<T>)_repositories[type];
        }

        /// <summary>
        /// Tüm değişiklikleri tek seferde veri tabanına kaydeder.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
