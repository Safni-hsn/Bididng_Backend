using Microsoft.AspNetCore.Mvc;
using SmartEnergyMarket.Services; // Make sure you import your service namespace
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SurplusBidRequest.DTOs;



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
        public async Task<IActionResult> GenerateBlocks(
    [FromQuery] double remainingEnergy,
    [FromQuery] DateTime blackoutStart,
    [FromQuery] DateTime blackoutEnd)
        {
            if (remainingEnergy <= 0)
                return BadRequest(new { message = "Remaining energy must be greater than 0." });

            if (blackoutEnd <= blackoutStart)
                return BadRequest(new { message = "Invalid blackout time range." });

            var blocks = await _surplusService.GenerateSurplusBlocksAsync(remainingEnergy, blackoutStart, blackoutEnd);
            return Ok(blocks);
        }

        // ✅ NEW GET endpoint: /api/surplus/blocks
        [HttpGet("blocks")]
        public async Task<IActionResult> GetBlocksForBlackout([FromQuery] DateTime blackoutStart, [FromQuery] DateTime blackoutEnd)
        {
            var blocks = await _surplusService.GetBlocksForBlackoutAsync(blackoutStart, blackoutEnd);
            return Ok(blocks);
        }

        [Authorize]
        [HttpPost("submit-bid")]
        public async Task<IActionResult> SubmitBid([FromBody] SurplusBidRequestDto bidRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _surplusService.SubmitBidAsync(userId, bidRequest);

            if (result == "Bid submitted successfully.")
                return Ok(new { message = result });

            return BadRequest(new { message = result });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all-bids")]
        public async Task<IActionResult> GetAllBids()
        {
            var bids = await _surplusService.GetAllBidsAsync();
            return Ok(bids);
        }
    }
}
