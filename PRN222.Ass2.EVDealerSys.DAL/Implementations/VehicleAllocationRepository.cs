using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations;

public class VehicleAllocationRepository : GenericRepository<VehicleAllocation>, IVehicleAllocationRepository
{
    public VehicleAllocationRepository(EvdealerDbContext context) : base(context) { }

    public async Task<IEnumerable<VehicleAllocation>> GetAllWithDetailsAsync()
    {
        return await _context.VehicleAllocations
            .Include(va => va.Vehicle)
            .Include(va => va.ToDealer)
            .Include(va => va.RequestedByUser)
            .Include(va => va.ApprovedByUser)
            .OrderByDescending(va => va.RequestDate)
            .ToListAsync();
    }

    public async Task<VehicleAllocation?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.VehicleAllocations
            .Include(va => va.Vehicle)
            .Include(va => va.ToDealer)
            .Include(va => va.RequestedByUser)
            .Include(va => va.ApprovedByUser)
            .FirstOrDefaultAsync(va => va.Id == id);
    }

    public async Task<IEnumerable<VehicleAllocation>> GetByDealerIdAsync(int dealerId)
    {
        return await _context.VehicleAllocations
            .Include(va => va.Vehicle)
            .Include(va => va.ToDealer)
            .Include(va => va.RequestedByUser)
            .Where(va => va.ToDealerId == dealerId)
            .OrderByDescending(va => va.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleAllocation>> GetByStatusAsync(int status)
    {
        return await _context.VehicleAllocations
            .Include(va => va.Vehicle)
            .Include(va => va.ToDealer)
            .Include(va => va.RequestedByUser)
            .Where(va => va.Status == status)
            .OrderByDescending(va => va.RequestDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<VehicleAllocation>> GetPendingRequestsAsync()
    {
        return await GetByStatusAsync(0); // Pending
    }

    public async Task<int> GetAvailableStockAsync(int vehicleId, string? color = null)
    {
        var query = _context.Inventories
            .Where(i => i.VehicleId == vehicleId && i.LocationType == 1); // EVM Factory

        if (!string.IsNullOrEmpty(color))
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.Color == color);
            if (vehicle == null) return 0;
        }

        var stock = await query.FirstOrDefaultAsync();
        return stock?.Quantity ?? 0;
    }

    public async Task<bool> HasSufficientStockAsync(int vehicleId, int quantity, string? color = null)
    {
        var availableStock = await GetAvailableStockAsync(vehicleId, color);
        return availableStock >= quantity;
    }
}
