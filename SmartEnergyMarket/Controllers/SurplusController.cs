using Microsoft.AspNetCore.Mvc;
using SmartEnergyMarket.Services; // Make sure you import your service namespace
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SurplusBidRequest.DTOs;
using SmartEnergyMarket.DTOs;
using SmartEnergyMarket.Data;
using Microsoft.EntityFrameworkCore;




namespace SmartEnergyMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurplusController : ControllerBase
    {
        private readonly SurplusService _surplusService;
        private readonly ApplicationDbContext _context;

         public SurplusController(ApplicationDbContext context, SurplusService surplusService){
        _surplusService = surplusService;
            _context = context;
    }


        // POST: api/surplus/generate-blocks?remainingEnergy=100

        // [HttpPost("generate-blocks")]
        // public async Task<IActionResult> GenerateBlocks([FromBody] GenerateBlocksDto dto)
        // {
        //     await _surplusService.GenerateSurplusBlocksAsync(
        //         dto.RemainingEnergy, dto.BlackoutStart, dto.BlackoutEnd
        //     );

        //     return Ok(new { message = "Blocks generated successfully." });
        // } 

      [HttpPost("generate-blocks-from-iqr")]
public async Task<IActionResult> GenerateBlocksFromIqr([FromBody] IqrResultDto dto)
{
    // Convert blackout times from string to DateTime
    if (!DateTime.TryParse(dto.BlackoutStart, out var blackoutStart) ||
        !DateTime.TryParse(dto.BlackoutEnd, out var blackoutEnd))
    {
        return BadRequest("Invalid blackout start or end time format.");
    }

    // Convert external data to your existing request
    var request = new BlockGenerationRequest
{
    UserCount = dto.CountAboveUpperBound,
    LowerBound = dto.UpperBound, 
    UpperBound = dto.MaxUsage,

    // ✅ Explicitly mark the times as UTC (without altering the actual time)
    BlackoutStartTime = DateTime.SpecifyKind(blackoutStart, DateTimeKind.Utc),
    BlackoutEndTime = DateTime.SpecifyKind(blackoutEnd, DateTimeKind.Utc)
};

    
    Console.WriteLine("📥 Incoming IQR DTO:");
Console.WriteLine($"🔢 UserCount: {dto.CountAboveUpperBound}");
Console.WriteLine($"📊 UpperBound: {dto.UpperBound}, MaxUsage: {dto.MaxUsage}");
Console.WriteLine($"🕒 BlackoutStart: {request.BlackoutStartTime}, BlackoutEnd: {dto.BlackoutEnd}");


    // Reuse your existing service
            var blocks = await _surplusService.GenerateBlocksFromUserCountAsync(request);
    return Ok(new { Message = $"✅ {blocks.Count} blocks created from IQR.", Blocks = blocks });
}




       [Authorize]
[HttpGet("blocks")]
public async Task<IActionResult> GetBlocks(
    [FromQuery] DateTime blackoutStart,
    [FromQuery] DateTime blackoutEnd)
{
    var blocksWithInfo = await _surplusService.GetBlocksWithBidInfoAsync(blackoutStart, blackoutEnd);
    return Ok(blocksWithInfo);
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
        [HttpPost("allocate-block/{blockId}")]
        public async Task<IActionResult> AllocateBlock(string blockId)
        {
            var result = await _surplusService.AllocateWinningBid(blockId);
            if (!result)
                return BadRequest("No bids or already allocated.");

            return Ok("Block allocated to the winner.");
        }

    [Authorize]
[HttpGet("next-bid-summary")]
public async Task<ActionResult<NextBidSummaryDto>> GetNextBidSummary()
{
    var userId = User.Claims
        .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
        .Select(c => c.Value)
        .FirstOrDefault();

    if (string.IsNullOrEmpty(userId))
        return Unauthorized("User ID not found in token.");

    var now = DateTime.UtcNow;

    // ✅ Step 1: Find next blackout block that user has bid on and is upcoming
    var nextUserBidBlock = await _context.SurplusBids
        .Where(b => b.UserId == userId && b.Block.BlackoutStartTime > now && !b.Block.IsAllocated)
        .OrderBy(b => b.Block.BlackoutStartTime)
        .Select(b => b.Block)
        .FirstOrDefaultAsync();

    if (nextUserBidBlock == null)
        return Ok(new NextBidSummaryDto { HasBid = false });

    // ✅ Step 2: Get all bids for that block
    var bids = await _context.SurplusBids
        .Where(b => b.BlockId == nextUserBidBlock.BlockId)
        .ToListAsync();

    var userBid = bids.FirstOrDefault(b => b.UserId == userId);
    var highestBid = bids.OrderByDescending(b => b.PricePerKwh).FirstOrDefault();

    var dto = new NextBidSummaryDto
    {
        NextBlackoutStart = nextUserBidBlock.BlackoutStartTime,
        YourBidPrice = userBid?.PricePerKwh,
        HighestBidPrice = highestBid?.PricePerKwh,
        HasBid = true,
        BlockSizeKwh = nextUserBidBlock.BlockSizeKwh
    };

    return Ok(dto);
}







    }
}
