namespace FixEngine.Models
{
    public class Quote
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public int Digits { get; set; }
    }
}
