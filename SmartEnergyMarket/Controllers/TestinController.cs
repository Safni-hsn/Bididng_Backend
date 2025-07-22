using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartEnergyMarket.Hubs;

[ApiController]
[Route("api/[controller]")]
public class NotificationTestController : ControllerBase
{
    private readonly IHubContext<AllocationHub> _hubContext;

    public NotificationTestController(IHubContext<AllocationHub> hubContext)
    {
        _hubContext = hubContext;
    }

[HttpPost("send")]
public async Task<IActionResult> SendTestNotification([FromBody] string userId)
{
    // Optional: Validate userId is a GUID
    if (!Guid.TryParse(userId, out var _))
    {
        return BadRequest("❌ Invalid userId format (must be GUID).");
    }

    // Log for debugging
    Console.WriteLine($"📤 Sending SignalR notification to user: {userId}");

    await _hubContext.Clients.User(userId).SendAsync("ReceiveAllocation", new
    {
        BlockId = "TEST-BLOCK",
        BidAmount = 2.5,
        Time = DateTime.UtcNow
    });

    return Ok($"✅ Notification sent to user: {userId}");
}

    
    [HttpPost("broadcast")]
public async Task<IActionResult> Broadcast()
{
    await _hubContext.Clients.All.SendAsync("ReceiveAllocation", new
    {
        BlockId = "BROADCAST-BLOCK",
        BidAmount = 9.9,
        Time = DateTime.UtcNow
    });

    return Ok("✅ Broadcast sent to ALL connected clients.");
}

}
