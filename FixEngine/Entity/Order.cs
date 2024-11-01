using Common;

namespace FixEngine.Entity
{
    public class Order
    {
        public int Id { get; set; }
        public RiskUser RiskUser { get; set; }
        public int RiskUserId { get; set; }
        public Position Position { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public DateTime EntryTime { get; set; } = DateTime.UtcNow;
        public DateTime CloseTime { get; set; }
        public string Status { get; set; } // Open Or Close
        public string GatewayType { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal FinalProfit { get; set; }
        public decimal FinalLoss { get; set; }
    }
}
