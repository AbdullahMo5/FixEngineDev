using FixEngine.Data;
using FixEngine.Entity;
using Microsoft.EntityFrameworkCore;

namespace FixEngine.Services
{
    public class GroupService : GenericService<Group>, IGroupService
    {
        private readonly DatabaseContext _context;

        public GroupService(DatabaseContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Group?> GetGroupByNameAsync(string groupName)
            => await _context.Groups.FirstOrDefaultAsync(g => g.Name.ToLower() == groupName.ToLower());
    }
}
