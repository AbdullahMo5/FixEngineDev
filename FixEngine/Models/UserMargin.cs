using FixEngine.Entity;
using FixEngine.Models.Simulation;

namespace FixEngine.Models
{
    public class UserMargin
    {
        public int RiskUserId { get; set; }
        public int GroupId { get; set; }
        public decimal Balance { get; set; }
        public decimal PNL { get; set; }
        public decimal Equity { get; set; }
        public decimal Leverage { get; set; }
        public decimal PoseSize { get; set; }
        public decimal MarginUsed { get; set; }
        public List<Position> UnFilledPositions { get; set; } = new List<Position> { };
        public List<Position> FilledPositions { get; set; } = new List<Position> { };
        public Dictionary<int?, SymbolBook> SymboolBook { get; set; } = new Dictionary<int?, SymbolBook>();
    }
}
