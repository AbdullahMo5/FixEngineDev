using FixEngine.Entity;

namespace FixEngine.Models
{
    public class UserMargin
    {
        public int RiskUserId { get; set; }
        public decimal Balance { get; set; }
        public decimal PNL { get; set; }
        public decimal Equity { get; set; }
        public decimal Leverage { get; set; }
        public decimal PoseSize { get; set; }
        public List<Position> UnFilledPositions { get; set; } = new List<Position> { };
        public List<Position> FilledPositions { get; set; } = new List<Position> { };
    }
}
