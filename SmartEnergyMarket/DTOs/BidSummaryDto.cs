namespace SmartEnergyMarket.DTOs
{
    public class NextBidSummaryDto
    {
        public DateTime? NextBlackoutStart { get; set; }
        public double? YourBidPrice { get; set; }
        public double? HighestBidPrice { get; set; }
        public bool HasBid { get; set; }

        public double BlockSizeKwh { get; set; }
    
}

}