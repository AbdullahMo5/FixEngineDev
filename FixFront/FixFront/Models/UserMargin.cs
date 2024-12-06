namespace FixFront.Models
{
    public class UserMargin
    {
        public int RiskUserId { get; set; }
        public decimal MarginLevel { get; set; }
        public decimal PNL { get; set; }
        public decimal Equity { get; set; }
    }
}
