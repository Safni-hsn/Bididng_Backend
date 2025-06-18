using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartEnergyMarket.Models
{



public class SurplusBlock
{
    [Key]
    public string BlockId { get; set; } = Guid.NewGuid().ToString();

    public double BlockSizeKwh { get; set; }

        [JsonIgnore]
        public double AvailableEnergyKwh { get; set; } // ✅ Add this line

    public double MinBidPricePerKwh { get; set; }

    public DateTime BlackoutStartTime { get; set; }
    public DateTime BlackoutEndTime { get; set; }


    public DateTime BlockTime { get; set; }

    public bool IsAllocated { get; set; } = false;

    public string? AllocatedToUserId { get; set; }
}


}
