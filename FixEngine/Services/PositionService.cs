using FixEngine.Data;
using FixEngine.Entity;

namespace FixEngine.Services
{
    public class PositionService : GenericService<Position>, IPositionService
    {
        public PositionService(DatabaseContext context) : base(context)
        {
        }
    }
}
