using FixEngine.Data;
using FixEngine.Entity;

namespace FixEngine.Services
{
    public class OrderService : GenericService<Order>, IOrderService
    {
        public OrderService(DatabaseContext context) : base(context) { }
    }
}
