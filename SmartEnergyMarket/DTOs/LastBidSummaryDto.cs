namespace SmartEnergyMarket.DTOs
{
public class LastBidSummaryDto
{
    public string BlockId { get; set; } = null!;
    public double BidPrice { get; set; }
    public double HighestBidPrice { get; set; }
    public DateTime BidTime { get; set; }
    public bool IsAllocated { get; set; }
    public DateTime BlackoutStart { get; set; }
    public DateTime BlackoutEnd { get; set; }
    public double BlockSizeKwh { get; set; }
}

}