namespace SurplusBidRequest.DTOs
{
    public class SurplusBidRequestDto
    {
        public string BlockId { get; set; } = string.Empty;
        public double BidAmountKwh { get; set; }  // ✅ REQUIRED
        public double PricePerKwh { get; set; }   // ✅ REQUIRED
    }
}
