namespace SmartEnergyMarket.DTOs
{
public class BlockAllocationResult
{
    public string BlockId { get; set; } = string.Empty;

    public string UserId { get; set; }
    public double BidAmount { get; set; }
    public DateTime BidTime { get; set; }
}
}