namespace FixFront.Models
{
    public class UserMargin
    {
        public int RiskUserId { get; set; }
        public decimal Balance { get; set; }
        public decimal PNL { get; set; }
        public decimal Equity { get; set; }
        public decimal Leverage { get; set; }
        public decimal PoseSize { get; set; }
    }
}
