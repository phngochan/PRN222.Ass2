using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(EvdealerDbContext context) : base(context)
        {
        }

        // Async methods from HEAD version
        public async Task<List<Vehicle>> GetAllActiveAsync()
        {
            return await _context.Vehicles
                .Where(v => v.Status == 1) // Active vehicles only
                .OrderBy(v => v.Model)
                .ThenBy(v => v.Version)
                .ToListAsync();
        }
        public override async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Inventories)
                    .ThenInclude(i => i.Dealer)
                .Include(v => v.OrderItems)
                .OrderBy(v => v.Model)
                .ToListAsync();
        }
        public async Task<Vehicle?> GetByIdWithRelaAsync(int id)
        {
            return await _context.Vehicles
                .Include(c => c.Inventories)
                .Include(c => c.OrderItems)
                .Include(c => c.VehicleAllocations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<Vehicle> AddAsync(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return vehicle;
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync()
        {
            return await _context.Vehicles
                .Where(v => v.Status == 1) // Active vehicles
                .OrderBy(v => v.Model)
                .ThenBy(v => v.Version)
                .ToListAsync();
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Vehicles.AnyAsync(v => v.Id == id);
        }

        public async Task<(List<Vehicle> Items, int TotalCount)> GetWithPaginationAsync(
            string? searchModel, string? searchVersion, int? filterStatus, string? filterColor,
            int page, int pageSize)
        {
            var query = _context.Vehicles.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchModel))
                query = query.Where(v => v.Model.Contains(searchModel));

            if (!string.IsNullOrWhiteSpace(searchVersion))
                query = query.Where(v => v.Version.Contains(searchVersion));

            if (filterStatus.HasValue)
                query = query.Where(v => v.Status == filterStatus.Value);

            if (!string.IsNullOrWhiteSpace(filterColor))
                query = query.Where(v => v.Color.Contains(filterColor));

            var totalCount = await query.CountAsync();

            var items = await query
                    .OrderBy(v => v.Model)
                    .ThenBy(v => v.Version)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                    .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Vehicle>> SearchAsync(string? searchTerm)
        {
            var query = _context.Vehicles
                .Where(v => v.Status == 1)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v => v.Model.Contains(searchTerm)
                    || v.Version.Contains(searchTerm)
                    || v.Color.Contains(searchTerm));
            }

            return await query
                .OrderBy(v => v.Model)
                .ThenBy(v => v.Version)
                .ToListAsync();
        }

        public override IEnumerable<Vehicle> GetAll()
        {
            return _context.Vehicles
                .Include(v => v.Inventories)
                .ToList();
        }

        public async Task<bool> CheckDuplicateAsync(string model, string version, string color, int? excludeId = null)
        {
            var query = _context.Vehicles
                .Where(v => v.Model == model && v.Version == version && v.Color == color);

            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);

            return await query.AnyAsync();
        }
        public async Task<List<string>> GetDistinctModelsAsync()
        {
            return await _context.Vehicles
                .Select(v => v.Model)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctColorsAsync()
        {
            return await _context.Vehicles
                .Select(v => v.Color)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        // Inventory management methods from dev version
        public Inventory? GetInventoryByVehicle(int vehicleId)
        {
            return _context.Inventories.FirstOrDefault(i => i.VehicleId == vehicleId && i.LocationType == 1);
        }

        public async Task<List<Vehicle>> GetByStatusAsync(int status)
        {
            return await _context.Vehicles
                .Where(v => v.Status == status)
                .OrderBy(v => v.Model)
                .ToListAsync();
        }
        public async Task<Inventory?> GetInventoryByVehicleAsync(int vehicleId)
        {
            return await _context.Inventories.FirstOrDefaultAsync(i => i.VehicleId == vehicleId && i.LocationType == 1);
        }

        public void UpdateInventory(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            _context.SaveChanges();
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> HasRelatedDataAsync(int vehicleId)
        {
            // Check if vehicle has any related inventory or orders
            var hasInventory = await _context.Inventories.AnyAsync(i => i.VehicleId == vehicleId);
            var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.VehicleId == vehicleId);
            var hasAllocations = await _context.VehicleAllocations.AnyAsync(va => va.VehicleId == vehicleId);

            return hasInventory || hasOrders || hasAllocations;
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Vehicles.CountAsync();
        }

        public async Task<int> GetCountByStatusAsync(int status)
        {
            return await _context.Vehicles.CountAsync(v => v.Status == status);
        }
    }
}
