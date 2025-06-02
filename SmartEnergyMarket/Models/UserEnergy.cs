namespace SmartEnergyMarket.Models{
public class UserEnergy
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Hour { get; set; }
    public double ActualUsageKWH { get; set; }
}
}