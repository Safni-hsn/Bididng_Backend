namespace SmartEnergyMarket.DTOs
{
public class BidDto
{
    public string UserName { get; set; }
    public double PricePerKwh { get; set; }
    public double BidAmountKwh { get; set; }
    public DateTime BidTime { get; set; }
}


}