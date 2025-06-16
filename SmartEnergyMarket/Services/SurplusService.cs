using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartEnergyMarket.Data;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Models;

namespace SmartEnergyMarket.Services
{

    public class SurplusService
    {
        private readonly ApplicationDbContext _context;

        public SurplusService(ApplicationDbContext context)
        {
            _context = context;
        }

       public async Task<List<SurplusBlock>> GenerateSurplusBlocksAsync(double remainingEnergyKwh)
{
    var blocks = new List<SurplusBlock>();
    var rand = new Random();

    int numberOfBlocks = rand.Next(3, 8); // Random between 3 and 7 blocks
    double totalAssigned = 0;

    for (int i = 0; i < numberOfBlocks; i++)
    {
        double remaining = remainingEnergyKwh - totalAssigned;

        // If it's the last block, assign all remaining energy
        double blockSize = (i == numberOfBlocks - 1)
            ? Math.Round(remaining, 2)
            : Math.Round(rand.NextDouble() * (remaining / 2) + 5, 2); // Random block size ≥5 kWh

        totalAssigned += blockSize;

        var block = new SurplusBlock
        {
            BlockId = Guid.NewGuid().ToString(),
            BlockSizeKwh = blockSize,
            AvailableEnergyKwh = blockSize,
            MinBidPricePerKwh = Math.Round(rand.NextDouble() * 3 + 7, 2), // ₹7.00 - ₹10.00
            BlockTime = DateTime.UtcNow,
            IsAllocated = false
        };

        _context.SurplusBlocks.Add(block);
        blocks.Add(block);
    }

    await _context.SaveChangesAsync();
    return blocks;
}


          public async Task<List<SurplusBlock>> GetAllSurplusBlocksAsync()
        {
            return await _context.SurplusBlocks.ToListAsync();
        }
    }
}