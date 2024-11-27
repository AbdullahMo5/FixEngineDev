namespace FixFront.Models
{
    public class Position
    {
        public int Index { get; set; }
        public long Id { get; init; }

        public int SymbolId { get; init; }

        public int RiskUserId { get; set; }

        public string SymbolName { get; set; }

        public decimal EntryPrice { get; init; }

        public decimal Volume { get; init; }

        public string TradeSide { get; init; }

        public decimal Profit { get; set; }
        public decimal StopLoss { get; init; }

        public decimal TakeProfit { get; init; }

        public bool? TrailingStopLoss { get; init; }

        public string StopLossTriggerMethod { get; init; }

        public bool? GuaranteedStopLoss { get; init; }
        public bool IsEmpty { get; set; }
    }
}
