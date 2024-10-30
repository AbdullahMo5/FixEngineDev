using System.Linq.Expressions;

namespace FixEngine.Services
{
    public interface IGenericService<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<int> AddAsync(T entity);
        Task<int> Update(T entity);
        Task<int> Delete(T entity);
        Task<bool> IsExist(Expression<Func<T, bool>> predicate);
    }
}
