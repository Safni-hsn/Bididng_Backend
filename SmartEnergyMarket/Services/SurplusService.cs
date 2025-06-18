using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartEnergyMarket.Data;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Models;
using SmartEnergyMarket.DTOs;
using SurplusBidRequest.DTOs;


namespace SmartEnergyMarket.Services
{

    public class SurplusService
    {
        private readonly ApplicationDbContext _context;

        public SurplusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SurplusBlock>> GenerateSurplusBlocksAsync(
    double remainingEnergyKwh, DateTime blackoutStartTime, DateTime blackoutEndTime)
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
                    BlackoutStartTime = blackoutStartTime,
                    BlackoutEndTime = blackoutEndTime,
                    IsAllocated = false
                };

                _context.SurplusBlocks.Add(block);
                blocks.Add(block);
            }

            await _context.SaveChangesAsync();
            return blocks;
        }


       public async Task<List<SurplusBlock>> GetBlocksForBlackoutAsync(DateTime blackoutStart, DateTime blackoutEnd)
{
    return await _context.SurplusBlocks
        .Where(b => b.BlackoutStartTime == blackoutStart && b.BlackoutEndTime == blackoutEnd)
        .ToListAsync();
}

        public async Task<string> SubmitBidAsync(string userId, SurplusBidRequestDto bidRequest)
        {
            var block = await _context.SurplusBlocks.FirstOrDefaultAsync(b => b.BlockId == bidRequest.BlockId);
            if (block == null)
                return "Block not found.";

            if (bidRequest.BidAmountKwh <= 0 || bidRequest.PricePerKwh < block.MinBidPricePerKwh)
                return "Invalid bid amount or price.";

            var existingBid = await _context.SurplusBids
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BlockId == bidRequest.BlockId);
            if (existingBid != null)
                return "You have already bid for this block.";

            var newBid = new SurplusBid
            {
                BlockId = block.BlockId,
                UserId = userId,
                BidAmountKwh = bidRequest.BidAmountKwh,
                PricePerKwh = bidRequest.PricePerKwh,
                BidTime = DateTime.UtcNow
            };

            _context.SurplusBids.Add(newBid);
            await _context.SaveChangesAsync();

            return "Bid submitted successfully.";
        }

        public async Task<List<SurplusBid>> GetAllBidsAsync()
        {
            return await _context.SurplusBids
                .Include(b => b.Block)
                .Include(b => b.User)
                .ToListAsync();
        }
    }
}