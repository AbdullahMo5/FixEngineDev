using FixEngine.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
        }

    }
}
