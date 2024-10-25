namespace FixEngine.Entity
{
    public enum GatewayType { A_book, B_book }
    public class Gateway
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GatewayType Type { get; set; }
    }
}
