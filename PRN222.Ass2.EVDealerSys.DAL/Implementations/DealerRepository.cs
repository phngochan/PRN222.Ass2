using Microsoft.EntityFrameworkCore;

using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations
{
    public class DealerRepository : GenericRepository<Dealer>, IDealerRepository
    {
        public DealerRepository(EvdealerDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Dealer>> GetAllAsync()
        {
            return await _context.Dealers
                .Include(d => d.Customers)
                .Include(d => d.Inventories)
                    .ThenInclude(i => i.Vehicle)
                .Include(d => d.Orders)
                .Include(d => d.Users)
                .Include(d => d.VehicleAllocations)
                .ToListAsync();
        }

        public async Task<Dealer?> GetByIdAsync(int id)
        {
            return await _context.Dealers
                .Include(d => d.Customers)
                    .ThenInclude(c => c.Orders)
                .Include(d => d.Inventories)
                    .ThenInclude(i => i.Vehicle)
                .Include(d => d.Orders)
                .Include(d => d.Users)
                .Include(d => d.VehicleAllocations)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Dealers.AnyAsync(d => d.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Dealers.Where(d => d.Name == name);
            if (excludeId.HasValue)
                query = query.Where(d => d.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Dealer>> SearchAsync(string? name, string? region, string? address = null)
        {
            var query = _context.Dealers
                .Include(d => d.Customers)
                .Include(d => d.Inventories)
                .Include(d => d.Orders)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(d => d.Name!.Contains(name));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(d => d.Region!.Contains(region));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(d => d.Address!.Contains(address));

            return await query.ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Dealers.CountAsync();
        }

        public async Task<int> GetDealersWithCustomersCountAsync()
        {
            return await _context.Dealers
                .Where(d => d.Customers.Any())
                .CountAsync();
        }

        public async Task<int> GetDealersWithInventoryCountAsync()
        {
            return await _context.Dealers
                .Where(d => d.Inventories.Any())
                .CountAsync();
        }

        public async Task<IEnumerable<Dealer>> GetPagedAsync(int page, int pageSize, string? name, string? region, string? address = null)
        {
            var query = _context.Dealers
                .Include(d => d.Customers)
                .Include(d => d.Inventories)
                .Include(d => d.Orders)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(d => d.Name!.Contains(name));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(d => d.Region!.Contains(region));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(d => d.Address!.Contains(address));

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalPagesAsync(int pageSize, string? name, string? region, string? address = null)
        {
            var query = _context.Dealers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(d => d.Name!.Contains(name));

            if (!string.IsNullOrEmpty(region))
                query = query.Where(d => d.Region!.Contains(region));

            if (!string.IsNullOrEmpty(address))
                query = query.Where(d => d.Address!.Contains(address));

            var totalItems = await query.CountAsync();
            return (int)Math.Ceiling((double)totalItems / pageSize);
        }

        public async Task<IEnumerable<Dealer>> GetByRegionAsync(string region)
        {
            return await _context.Dealers
                .Where(d => d.Region == region)
                .Include(d => d.Customers)
                .Include(d => d.Inventories)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetDistinctRegionsOnlyAsync()
        {
            return await _context.Dealers
                .Where(d => !string.IsNullOrEmpty(d.Region))
                .Select(d => d.Region!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }

        public async Task<IEnumerable<Dealer>> GetDealersWithBasicInfoAsync()
        {
            return await _context.Dealers
                .Select(d => new Dealer
                {
                    Id = d.Id,
                    Name = d.Name,
                    Region = d.Region
                })
                .Where(d => !string.IsNullOrEmpty(d.Name))
                .OrderBy(d => d.Region)
                .ThenBy(d => d.Name)
                .ToListAsync();
        }

        // Get all distinct regions
        public async Task<List<string>> GetRegionsAsync()
        {
            return await _context.Dealers
                .Where(d => !string.IsNullOrEmpty(d.Region))
                .Select(d => d.Region!)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();
        }
    }
}
