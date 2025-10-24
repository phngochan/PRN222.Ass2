using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces;

public interface IVehicleAllocationRepository : IGenericRepository<VehicleAllocation>
{
    public Task<IEnumerable<VehicleAllocation>> GetAllWithDetailsAsync();
    public Task<VehicleAllocation?> GetByIdWithDetailsAsync(int id);
    public Task<IEnumerable<VehicleAllocation>> GetByDealerIdAsync(int dealerId);
    public Task<IEnumerable<VehicleAllocation>> GetByStatusAsync(int status);
    public Task<IEnumerable<VehicleAllocation>> GetPendingRequestsAsync();
    public Task<int> GetAvailableStockAsync(int vehicleId, string? color = null);
    public Task<bool> HasSufficientStockAsync(int vehicleId, int quantity, string? color = null);
    public Task<IEnumerable<VehicleAllocation>> GetByUserIdAsync(int userId);
    public Task<IEnumerable<VehicleAllocation>> GetByDealerAndStatusAsync(int dealerId, int status);
}
