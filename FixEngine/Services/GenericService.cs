using FixEngine.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FixEngine.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly DatabaseContext _context;

        public GenericService(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<int> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            return await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        public async Task<bool> IsExist(Expression<Func<T, bool>> predicate)
            => await _context.Set<T>().AnyAsync(predicate);

        public async Task<int> Update(T entity)
        {
            _context.Set<T>().Update(entity);
            return await _context.SaveChangesAsync();
        }

    }
}
