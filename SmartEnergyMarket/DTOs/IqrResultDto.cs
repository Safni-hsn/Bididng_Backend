namespace SmartEnergyMarket.DTOs
{
    public class IqrResultDto
{
    public int BlackoutId { get; set; }
    public double UpperBound { get; set; }
    public int CountAboveUpperBound { get; set; }
    public double MaxUsage { get; set; }
    public string BlackoutStart { get; set; }
    public string BlackoutEnd { get; set; }
}

}