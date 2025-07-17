namespace SmartEnergyMarket.DTOs
{
    public class BlockWithBidInfoDto
{
    public string BlockId { get; set; }
    public double BlockSizeKwh { get; set; }
    public double MinBidPricePerKwh { get; set; }
    public DateTime BlackoutStartTime { get; set; }
    public DateTime BlackoutEndTime { get; set; }

    public bool HasBids { get; set; }
    public double? HighestBidPrice { get; set; }
}


}