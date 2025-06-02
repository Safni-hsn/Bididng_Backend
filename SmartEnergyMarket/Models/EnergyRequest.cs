namespace SmartEnergyMarket.Models
{
public class EnergyRequest

{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public double RequestedKWH { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
}
