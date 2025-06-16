using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Models;

namespace SmartEnergyMarket.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ✅ Add this to support historical usage storage
        public DbSet<UserEnergy> UserEnergies { get; set; }
        public DbSet<SurplusBlock> SurplusBlocks { get; set; }
       public DbSet<SurplusBid> SurplusBids { get; set; }

    }
}
