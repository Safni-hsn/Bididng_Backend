namespace SmartEnergyMarket.DTOs
{
    public class BlockGenerationRequest
{
    public int UserCount { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public DateTime BlackoutStartTime { get; set; }
    public DateTime BlackoutEndTime { get; set; }
}

}