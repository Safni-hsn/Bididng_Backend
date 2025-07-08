namespace SmartEnergyMarket.DTOs
{
    public class BlockWithBidsDto
{
    public Guid BlockId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Capacity { get; set; }
    public List<BidDto> Bids { get; set; }
}
}