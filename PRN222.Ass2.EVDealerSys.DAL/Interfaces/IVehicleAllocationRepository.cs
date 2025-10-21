using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces;

public interface IVehicleAllocationRepository : IGenericRepository<VehicleAllocation>
{
    Task<IEnumerable<VehicleAllocation>> GetAllWithDetailsAsync();
    Task<VehicleAllocation?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<VehicleAllocation>> GetByDealerIdAsync(int dealerId);
    Task<IEnumerable<VehicleAllocation>> GetByStatusAsync(int status);
    Task<IEnumerable<VehicleAllocation>> GetPendingRequestsAsync();
    Task<int> GetAvailableStockAsync(int vehicleId, string? color = null);
    Task<bool> HasSufficientStockAsync(int vehicleId, int quantity, string? color = null);
}
