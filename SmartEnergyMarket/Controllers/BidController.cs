using Microsoft.AspNetCore.Mvc;
using SmartEnergyMarket.Data;

namespace SmartEnergyMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BidController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BidController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("available-energy")]
        public IActionResult GetAvailableEnergy([FromQuery] string start, [FromQuery] string end)
        {
            if (!TimeSpan.TryParse(start, out TimeSpan blackoutStart) || !TimeSpan.TryParse(end, out TimeSpan blackoutEnd))
            {
                return BadRequest(new { message = "Invalid time format. Use HH:mm (e.g., 08:00)." });
            }

            if (blackoutEnd <= blackoutStart)
            {
                return BadRequest(new { message = "End time must be later than start time." });
            }

            // Step 1: Count registered users
            int totalUsers = _context.Users.Count();

            // Step 2: Calculate duration in hours
            var blackoutDurationHours = (blackoutEnd - blackoutStart).TotalHours;

            // Step 3: Set fixed available energy per user per hour
            const double energyPerUserPerHour = 1.2;

            // Step 4: Calculate total available energy
            double totalAvailableEnergy = totalUsers * blackoutDurationHours * energyPerUserPerHour;

            return Ok(new
            {
                blackoutStart = start,
                blackoutEnd = end,
                totalUsers,
                totalAvailableEnergy = Math.Round(totalAvailableEnergy, 2)
            });
        }
    }
}
