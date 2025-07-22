using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string? ReferenceNumber { get; set; }
    // You can add custom fields like Role, DisplayName etc.
}
