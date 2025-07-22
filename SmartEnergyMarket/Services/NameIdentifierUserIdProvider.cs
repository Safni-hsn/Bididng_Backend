using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System;
using System.Linq;

public class NameIdentifierUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var userId = connection.User?.Claims
            .Where(c => c.Type == ClaimTypes.NameIdentifier && Guid.TryParse(c.Value, out _))
            .Select(c => c.Value)
            .FirstOrDefault();

        Console.WriteLine("🔐 SignalR resolved userId: " + userId);
        return userId;
    }
}
