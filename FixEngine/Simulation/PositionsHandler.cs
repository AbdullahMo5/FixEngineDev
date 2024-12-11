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
                PositionId = Guid.NewGuid(),
                SymbolName = newOrderRequest.SymbolName,
                SymbolId = newOrderRequest.SymbolId ?? 0,
                TradeSide = newOrderRequest.TradeSide,
                Volume = newOrderRequest.Quantity,
                RiskUserId = newOrderRequest.RiskUserId
            };

            return newPos;
        }

        public static Position ClosePosition(List<Position> positions, string positionId)
        {
            var oldPos = positions.FirstOrDefault(p => p.PositionId.ToString() == positionId);
            if (oldPos == null) return null;

            var newPos = new Position
            {
                PositionId = Guid.NewGuid(),
                SymbolName = oldPos.SymbolName,
                SymbolId = oldPos.SymbolId,
                TradeSide = oldPos.TradeSide,
                Volume = oldPos.Volume,
                RiskUserId = oldPos.RiskUserId
            };

            return newPos;
        }
    }
}
