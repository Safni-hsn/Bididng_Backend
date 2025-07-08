using Microsoft.AspNetCore.Mvc;
using SmartEnergyMarket.Services; // Make sure you import your service namespace
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SurplusBidRequest.DTOs;
using SmartEnergyMarket.DTOs;



namespace SmartEnergyMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurplusController : ControllerBase
    {
        private readonly SurplusService _surplusService;

        public SurplusController(SurplusService surplusService)
        {
            _surplusService = surplusService;
        }

        // POST: api/surplus/generate-blocks?remainingEnergy=100
        
       [HttpPost("generate-blocks")]
public async Task<IActionResult> GenerateBlocks([FromBody] GenerateBlocksDto dto)
{
    await _surplusService.GenerateSurplusBlocksAsync(
        dto.RemainingEnergy, dto.BlackoutStart, dto.BlackoutEnd
    );

    return Ok(new { message = "Blocks generated successfully." });
}


[Authorize]
[HttpGet("blocks")]
public async Task<IActionResult> GetBlocks(
    [FromQuery] DateTime blackoutStart,
    [FromQuery] DateTime blackoutEnd)
{
    var blocks = await _surplusService.GetBlocksForBlackoutAsync(blackoutStart, blackoutEnd);
    return Ok(blocks);
}


        [Authorize]
[HttpPost("submit-bid")]
public async Task<IActionResult> SubmitBid([FromBody] SurplusBidRequestDto bidRequest)
{
    // 🔍 Print all claims to see what the JWT contains
    foreach (var claim in User.Claims)
    {
        Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
    }

    // ✅ Extract the correct claim manually
    var userId = User.Claims
    .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
    .Select(c => c.Value)
    .FirstOrDefault();
Console.WriteLine($"✅ Extracted UserId from JWT: {userId}");


    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    var result = await _surplusService.SubmitBidAsync(userId, bidRequest);

    if (result == "Bid submitted successfully.")
        return Ok(new { message = result });

    return BadRequest(new { message = result });
}




  [HttpGet("bids-by-blackout")]
public async Task<IActionResult> GetBidsByBlackout([FromQuery] DateTime blackoutStart, [FromQuery] DateTime blackoutEnd)
{
    var bids = await _surplusService.GetBidsByBlackoutAsync(blackoutStart, blackoutEnd);
    return Ok(bids);
}


    }
}
