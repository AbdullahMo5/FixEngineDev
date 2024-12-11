namespace FixEngine.Entity
{
    public class Position
    {
        public int Id { get; set; }
        public DateTime TimeClose { get; set; } = DateTime.UtcNow;
        public Guid PositionId { get; set; }
        public Order Order { get; set; }
        public int OrderId { get; set; }
        public int RiskUserId { get; set; }
        public int SymbolId { get; set; }
        public string SymbolName { get; set; }
        public string TradeSide { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal Profit { get; set; }
        public decimal Volume { get; set; }
    }
}
