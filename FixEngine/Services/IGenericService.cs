namespace FixEngine.Services
{
    public interface IGenericService<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<int> AddAsync(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}
