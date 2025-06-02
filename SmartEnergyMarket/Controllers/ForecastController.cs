using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Data;
using SmartEnergyMarket.Models;
using System.Globalization;
using System.Security.Claims;

namespace SmartEnergyMarket.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ForecastController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ForecastController(ApplicationDbContext context)
    {
        _context = context;
    }

[HttpGet("predict")]
[Authorize]
public IActionResult Predict([FromQuery] string start, [FromQuery] string end)
{
    if (!TimeSpan.TryParse(start, out var startTime) || !TimeSpan.TryParse(end, out var endTime))
        return BadRequest(new { message = "Invalid time format. Use HH:mm format." });

    // Calculate duration in hours
    var durationHours = (endTime - startTime).TotalHours;
    if (durationHours <= 0)
        return BadRequest(new { message = "End time must be after start time." });

    // Generate a random predicted value between 1.5 and 2.5 per hour
    var rand = new Random();
    double usagePerHour = rand.NextDouble() * (2.5 - 1.5) + 1.5;
    double totalPredictedUsage = Math.Round(usagePerHour * durationHours, 2);

    return Ok(new { predictedUsageKWh = totalPredictedUsage });
}






    }

