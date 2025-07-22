using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartEnergyMarket.Hubs
{
    public class AllocationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
            .Select(c => c.Value)
            .FirstOrDefault();

            Console.WriteLine($"✅ SignalR user connected: {userId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
            .Select(c => c.Value)
            .FirstOrDefault();

            Console.WriteLine($"❌ SignalR user disconnected: {userId}");

            await base.OnDisconnectedAsync(exception);
        }
    }
}
