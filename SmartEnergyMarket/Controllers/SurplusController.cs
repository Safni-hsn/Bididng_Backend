using Microsoft.AspNetCore.Mvc;
using SmartEnergyMarket.Services; // Make sure you import your service namespace
using System.Threading.Tasks;

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
        public async Task<IActionResult> GenerateBlocks([FromQuery] double remainingEnergy)
        {
            if (remainingEnergy <= 0)
                return BadRequest(new { message = "Remaining energy must be greater than 0." });

            var blocks = await _surplusService.GenerateSurplusBlocksAsync(remainingEnergy);
            return Ok(blocks);
        }

        // ✅ NEW GET endpoint: /api/surplus/blocks
        [HttpGet("blocks")]
        public async Task<IActionResult> GetAllBlocks()
        {
            var blocks = await _surplusService.GetAllSurplusBlocksAsync();
            return Ok(blocks);
        }
    }
}
