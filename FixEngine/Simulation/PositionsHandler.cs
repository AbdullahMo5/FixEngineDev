using FixEngine.Entity;
using FixEngine.Services;

namespace FixEngine.Simulation
{
    public static class PositionsHandler
    {
        public static Position NewUnFilledPosition(NewOrderRequestParameters newOrderRequest)
        {
            var newPos = new Position
            {
                SymbolName = newOrderRequest.SymbolName,
                SymbolId = newOrderRequest.SymbolId ?? 0,
                TradeSide = newOrderRequest.TradeSide,
                Volume = newOrderRequest.Quantity,
                RiskUserId = newOrderRequest.RiskUserId
            };

            return newPos;
        }
    }
}
