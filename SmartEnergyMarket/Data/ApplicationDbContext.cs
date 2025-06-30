using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Models;

namespace SmartEnergyMarket.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UserEnergy> UserEnergies { get; set; }
        public DbSet<SurplusBlock> SurplusBlocks { get; set; }
        public DbSet<SurplusBid> SurplusBids { get; set; }
    }
}
