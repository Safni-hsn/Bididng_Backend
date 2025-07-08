namespace SmartEnergyMarket.DTOs
{
    public class GenerateBlocksDto
{
    public double RemainingEnergy { get; set; }
    public DateTime BlackoutStart { get; set; }
    public DateTime BlackoutEnd { get; set; }
}

}