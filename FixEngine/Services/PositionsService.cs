using FixEngine.Data;
using FixEngine.Entity;

namespace FixEngine.Services
{
    public class PositionsService : GenericService<Position>, IPositionsService
    {
        public PositionsService(DatabaseContext context) : base(context)
        {
        }
    }
}
