using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartEnergyMarket.Data;
using SmartEnergyMarket.Models;
using System.Text;
using SmartEnergyMarket.Services;
using SmartEnergyMarket.Hubs;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SurplusService>();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add Authentication (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // ✅ THIS PART IS REQUIRED for SignalR JWT support
     options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/allocationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // ✅ This makes SignalR's Clients.User(userId) work
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                context.Principal.AddIdentity(new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("UserIdentifier", userId)
                }));
            }

            return Task.CompletedTask;
        }
    };
});

// CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5174", "http://localhost:5173") // frontend ports
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // needed for JWT + SignalR
        });
});

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();


// Add Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        var exists = await roleManager.RoleExistsAsync(role);
        if (!exists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication(); // 🛡 Must be before Authorization
app.UseAuthorization();

app.MapControllers();
app.MapHub<AllocationHub>("/allocationHub"); // ✅ SignalR endpoint

app.Run();
