using FixEngine.Entity;

namespace FixEngine.Services
{
    public interface IGroupService : IGenericService<Group>
    {
        Task<Group?> GetGroupByNameAsync(string groupName);
        Task<bool> IsGroupExistsAsync(string groupName);
    }
}
