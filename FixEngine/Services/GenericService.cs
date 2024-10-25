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

        public Task Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();

        public async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        public Task Update(T entity)
        {
            throw new NotImplementedException();
        }


    }
}
