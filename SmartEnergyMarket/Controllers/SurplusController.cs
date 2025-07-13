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
    // ✅ Extract userId from JWT claims
    var userId = User.Claims
        .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
        .Select(c => c.Value)
        .FirstOrDefault();

    if (string.IsNullOrEmpty(userId))
        return Unauthorized("User ID not found in token.");

    var now = DateTime.UtcNow;

    // ✅ Step 1: Find next upcoming blackout block
    var today = DateTime.UtcNow.Date;

var nextBlock = await _context.SurplusBlocks
    .Where(b =>
        b.BlackoutStartTime.Date == today &&               // 🔹 Only today's blackouts
        b.BlackoutEndTime > DateTime.UtcNow &&             // 🔹 Still upcoming
        !b.IsAllocated                                     // 🔹 Not already allocated
    )
    .OrderBy(b => b.BlackoutStartTime)                     // 🔹 Earliest next
    .FirstOrDefaultAsync();


    if (nextBlock == null)
    {
        return Ok(new NextBidSummaryDto { HasBid = false });
    }

    // ✅ Step 2: Get all bids for that block
    var bids = await _context.SurplusBids
        .Where(b => b.BlockId == nextBlock.BlockId)
        .ToListAsync();

        

    var userBid = bids.FirstOrDefault(b => b.UserId == userId);
    var highestBid = bids.OrderByDescending(b => b.PricePerKwh).FirstOrDefault();

    var dto = new NextBidSummaryDto
    {
        NextBlackoutStart = nextBlock.BlackoutStartTime,
        YourBidPrice = userBid?.PricePerKwh,
        HighestBidPrice = highestBid?.PricePerKwh,
        HasBid = userBid != null
    };

    return Ok(dto);
}






    }
}
