using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartEnergyMarket.Data;
using Microsoft.EntityFrameworkCore;
using SmartEnergyMarket.Models;
using SmartEnergyMarket.DTOs;
using SurplusBidRequest.DTOs;
using Microsoft.AspNetCore.SignalR;
using SmartEnergyMarket.Hubs;



namespace SmartEnergyMarket.Services
{

    public class SurplusService
    {
        private readonly ApplicationDbContext _context;
private readonly IHubContext<AllocationHub> _hubContext;

public SurplusService(ApplicationDbContext context, IHubContext<AllocationHub> hubContext)
{
    _context = context;
    _hubContext = hubContext;
}


        //     public async Task<List<SurplusBlock>> GenerateSurplusBlocksAsync(
        // double remainingEnergyKwh, DateTime blackoutStartTime, DateTime blackoutEndTime)
        //     {
        //         var blocks = new List<SurplusBlock>();
        //         var rand = new Random();

        //         int numberOfBlocks = rand.Next(3, 8); // Random between 3 and 7 blocks
        //         double totalAssigned = 0;

        //         for (int i = 0; i < numberOfBlocks; i++)
        //         {
        //             double remaining = remainingEnergyKwh - totalAssigned;

        //             // If it's the last block, assign all remaining energy
        //             double blockSize = (i == numberOfBlocks - 1)
        //                 ? Math.Round(remaining, 2)
        //                 : Math.Round(rand.NextDouble() * (remaining / 2) + 5, 2); // Random block size ≥5 kWh

        //             totalAssigned += blockSize;

        //             var block = new SurplusBlock
        //             {
        //                 BlockId = Guid.NewGuid().ToString(),
        //                 BlockSizeKwh = blockSize,
        //                 AvailableEnergyKwh = blockSize,
        //                 MinBidPricePerKwh = Math.Round(rand.NextDouble() * 3 + 7, 2), // ₹7.00 - ₹10.00
        //                 BlockTime = DateTime.UtcNow,
        //                 BlackoutStartTime = blackoutStartTime,
        //                 BlackoutEndTime = blackoutEndTime,
        //                 IsAllocated = false
        //             };

        //             _context.SurplusBlocks.Add(block);
        //             blocks.Add(block);
        //         }

        //         await _context.SaveChangesAsync();
        //         return blocks;
        //     }
    
    public async Task<List<SurplusBlock>> GenerateBlocksFromUserCountAsync(BlockGenerationRequest request)
{
    var random = new Random();
    var blocks = new List<SurplusBlock>();

    for (int i = 0; i < request.UserCount; i++)
    {
        // Random size within range and rounded to 1 decimal (e.g., 2.3)
        double size = Math.Round(
            request.LowerBound + random.NextDouble() * (request.UpperBound - request.LowerBound),
            1
        );

        var block = new SurplusBlock
        {
            BlockSizeKwh = size,
            MinBidPricePerKwh = 5.0, // Set your default or dynamic min price
            BlackoutStartTime = request.BlackoutStartTime,
            BlackoutEndTime = request.BlackoutEndTime,
            BlockTime = DateTime.UtcNow
        };

        blocks.Add(block);
        _context.SurplusBlocks.Add(block);
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
            Console.WriteLine($"🔍 Extracted UserId from JWT: {userId}");

            // ✅ Step 1: Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return "❌ User not found in database.";
            }

            // ✅ Step 2: Check if block exists
            var block = await _context.SurplusBlocks.FindAsync(bidRequest.BlockId);
            if (block == null)
            {
                return "❌ Surplus block not found.";
            }

            // ✅ Step 3: Create and save the bid
            var bid = new SurplusBid
            {
                BlockId = block.BlockId,
                UserId = user.Id,
                BidAmountKwh = bidRequest.BidAmountKwh,
                PricePerKwh = bidRequest.PricePerKwh,
                BidTime = DateTime.UtcNow
            };

            _context.SurplusBids.Add(bid);
            await _context.SaveChangesAsync();

            return "Bid submitted successfully.";
        }




        public async Task<List<SurplusBid>> GetBidsByBlackoutAsync(DateTime blackoutStart, DateTime blackoutEnd)
        {
            return await _context.SurplusBids
                .Include(b => b.Block)
                .Include(b => b.User)
                .Where(b => b.Block.BlackoutStartTime == blackoutStart && b.Block.BlackoutEndTime == blackoutEnd)
                .OrderByDescending(b => b.PricePerKwh) // Highest price first
                .ToListAsync();
        }

        public async Task<List<BlockWithBidsDto>> GetBidsGroupedByBlockAsync(DateTime blackoutStart, DateTime blackoutEnd)
        {
            var blocks = await _context.SurplusBlocks
                .Where(b => b.BlackoutStartTime == blackoutStart && b.BlackoutEndTime == blackoutEnd)
                .ToListAsync();

            var blockIds = blocks.Select(b => b.BlockId).ToList();

            var bids = await _context.SurplusBids
                .Where(b => blockIds.Contains(b.BlockId))
                .Include(b => b.User)
                .OrderByDescending(b => b.PricePerKwh)
                .ToListAsync();

            var result = blocks.Select(block => new BlockWithBidsDto
            {
                BlockId = Guid.Parse(block.BlockId),
                StartTime = block.BlackoutStartTime,
                EndTime = block.BlackoutEndTime,
                Capacity = block.BlockSizeKwh,
                Bids = bids
                    .Where(b => b.BlockId == block.BlockId)
                    .Select(b => new BidDto
                    {
                        UserName = b.User.UserName,
                        PricePerKwh = b.PricePerKwh,
                        BidAmountKwh = b.BidAmountKwh,
                        BidTime = b.BidTime
                    }).ToList()
            }).ToList();

            return result;
        }

        public async Task<bool> AllocateWinningBid(string blockId)
        {
            var block = await _context.SurplusBlocks.FindAsync(blockId);
            if (block == null || block.IsAllocated) return false;

            var bids = await _context.SurplusBids
                .Where(b => b.BlockId == blockId)
                .OrderByDescending(b => b.PricePerKwh)
                .ThenBy(b => b.BidTime)
                .ToListAsync();

            var winningBid = bids.FirstOrDefault();
            if (winningBid == null) return false;

            // Allocate the block
            block.IsAllocated = true;
            block.AllocatedToUserId = winningBid.UserId;
            block.WinningBidId = winningBid.Id.ToString();


            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<BlockWithBidInfoDto>> GetBlocksWithBidInfoAsync(DateTime blackoutStart, DateTime blackoutEnd)
        {
            var blocks = await _context.SurplusBlocks
                .Where(b => b.BlackoutStartTime == blackoutStart && b.BlackoutEndTime == blackoutEnd)
                .ToListAsync();

            var blockIds = blocks.Select(b => b.BlockId).ToList();

            // Get all related bids in one query
            var bids = await _context.SurplusBids
                .Where(b => blockIds.Contains(b.BlockId))
                .ToListAsync();

            var blockDtos = blocks.Select(block =>
            {
                var relatedBids = bids.Where(b => b.BlockId == block.BlockId).ToList();
                var hasBids = relatedBids.Any();
                var highestBid = hasBids ? relatedBids.Max(b => b.PricePerKwh) : (double?)null;

                return new BlockWithBidInfoDto
                {
                    BlockId = block.BlockId,
                    BlockSizeKwh = block.BlockSizeKwh,
                    MinBidPricePerKwh = block.MinBidPricePerKwh,
                    BlackoutStartTime = block.BlackoutStartTime,
                    BlackoutEndTime = block.BlackoutEndTime,
                    HasBids = hasBids,
                    HighestBidPrice = highestBid
                };
            }).ToList();

            return blockDtos;
        }
public async Task<List<BlockAllocationResult>> AllocateBlocksByBlackoutAsync(DateTime blackoutStart, DateTime blackoutEnd)
{
    var bids = await _context.SurplusBids
        .Include(b => b.Block)
        .Where(b => b.Block.BlackoutStartTime == blackoutStart && b.Block.BlackoutEndTime == blackoutEnd)
        .OrderByDescending(b => b.PricePerKwh)
        .ThenBy(b => b.BidTime)
        .ToListAsync();

    var allocatedUsers = new HashSet<string>();
    var allocatedBlocks = new HashSet<string>();
    var finalAllocations = new List<BlockAllocationResult>();

    foreach (var bid in bids)
    {
        var userId = bid.UserId;
        var blockId = bid.BlockId!; // ✅ blockId is a string (GUID)

                if (!allocatedUsers.Contains(userId) && !allocatedBlocks.Contains(blockId))
                {
                    finalAllocations.Add(new BlockAllocationResult
                    {
                        BlockId = blockId, // ✅ Don't parse, just assign string directly
                        UserId = userId,
                        BidAmount = bid.PricePerKwh,
                        BlockSize = bid.Block.BlockSizeKwh,
                        BidTime = bid.BidTime
                    });

                    // Mark as allocated
                    allocatedUsers.Add(userId);
                    allocatedBlocks.Add(blockId);

                    bid.Block.AllocatedToUserId = userId;
                    bid.Block.IsAllocated = true;
                    bid.Block.WinningBidId = bid.Id.ToString(); // ✅ convert bid ID to string
            
                await _hubContext.Clients.User(userId).SendAsync("ReceiveAllocation", new
{
    BlockId = blockId,
    BidAmount = bid.PricePerKwh,
    BlockSize = bid.Block.BlockSizeKwh,
    Time = DateTime.UtcNow
});
        }
        
    }




    await _context.SaveChangesAsync();
    return finalAllocations;
}









    }
}