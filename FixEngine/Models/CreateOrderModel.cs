namespace FixEngine.Models
{
    public class CreateOrderModel
    {
        public decimal EntryPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal Quantity { get; set; }
        public DateTime CloseTime { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string ClOrdId { get; set; }
        public string? SymbolName { get; set; }
        public string? TradeSide { get; set; }
        public string GatewayType { get; set; }
        public int? SymbolId { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal FinalProfit { get; set; }
        public decimal FinalLoss { get; set; }
        public int RiskUserId { get; set; }
    }
}
