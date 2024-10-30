namespace FixEngine.Models
{
    public class CreateOrderModel
    {
        public decimal EntryPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public DateTime CloseTime { get; set; }
        public string Status { get; set; }
        public string GatewayType { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal FinalProfit { get; set; }
        public decimal FinalLoss { get; set; }
    }
}
