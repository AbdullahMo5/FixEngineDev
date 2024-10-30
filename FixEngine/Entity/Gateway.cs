namespace FixEngine.Entity
{
    public enum GatewayType { ABook, BBook }
    public class Gateway
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GatewayType Type { get; set; }
    }
}
