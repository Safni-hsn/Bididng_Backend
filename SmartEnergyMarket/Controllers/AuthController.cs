using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartEnergyMarket.Models;
using SmartEnergyMarket.DTOs;
using SmartEnergyMarket.Data; // ✅ Added for DbContext
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SmartEnergyMarket.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ✅ Injected DbContext
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        // ✅ Constructor updated to include _context
        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // ✅ Retrieve saved user (to get correct ID)
            var createdUser = await _userManager.FindByEmailAsync(dto.Email);

            var startDate = new DateTime(2025, 5, 20, 0, 0, 0, DateTimeKind.Utc); // Fixed starting date
            var random = new Random();

            for (int day = 0; day < 7; day++)
            {
                double dailyTotal = random.NextDouble() * (33 - 26) + 26;

                double[] weights = Enumerable.Range(0, 24)
                    .Select(_ => random.NextDouble() + 0.5)
                    .ToArray();

                double weightSum = weights.Sum();
                weights = weights.Select(w => w / weightSum).ToArray();

                for (int hour = 0; hour < 24; hour++)
                {
                    var usage = dailyTotal * weights[hour];
                    var utcHour = startDate.AddDays(day).AddHours(hour);

                    _context.UserEnergies.Add(new UserEnergy
                    {
                        UserId = createdUser.Id, // ✅ Use ID from saved user
                        Hour = utcHour,
                        ActualUsageKWH = Math.Round(usage, 2)
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered and usage data seeded." });
        }

        [HttpPost("register-admin")]
public async Task<IActionResult> RegisterAdmin(RegisterDto dto)
{
    var user = new ApplicationUser
    {
        UserName = dto.Username,
        Email = dto.Email
    };

    var result = await _userManager.CreateAsync(user, dto.Password);

    if (!result.Succeeded)
        return BadRequest(result.Errors);

    // ✅ Add the Admin role to the user
    await _userManager.AddToRoleAsync(user, "Admin");

    return Ok(new { message = "Admin registered successfully." });
}


        [HttpGet("debug-db")]
public IActionResult DebugDb()
{
    var db = _context.Database.GetDbConnection().Database;
    return Ok(new { ConnectedDatabase = db });
}



        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            
            var token = await GenerateJwtToken(user);
            return Ok(new { token });
        }

      private async Task<string> GenerateJwtToken(ApplicationUser user)
{
    var roles = await _userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
    };

    // Add roles to claims
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Issuer"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

        
        
    }
}
