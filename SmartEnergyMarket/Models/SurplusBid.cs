using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartEnergyMarket.Models;

public class SurplusBid
{
    public int Id { get; set; }

    public string BlockId { get; set; } = string.Empty;

    public SurplusBlock? Block { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public double BidAmountKwh { get; set; }

    public double PricePerKwh { get; set; }

    public DateTime BidTime { get; set; } = DateTime.UtcNow;
}
