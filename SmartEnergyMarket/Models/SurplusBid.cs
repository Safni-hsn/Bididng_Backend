using SmartEnergyMarket.Models; // Adjust based on your folder structure

public class SurplusBid
{
    public int Id { get; set; }
    public string BlockId { get; set; }
    public string UserId { get; set; }

    public double BidAmountKwh { get; set; }
    public double PricePerKwh { get; set; }

    public DateTime BidTime { get; set; } = DateTime.UtcNow;

    public ApplicationUser? User { get; set; }
    public SurplusBlock? Block { get; set; }

}
