using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        // Async methods
        Task<List<Vehicle>> GetAllActiveAsync();
        Task<Vehicle> AddAsync(Vehicle vehicle);
        Task<List<Vehicle>> GetAvailableVehiclesAsync();
        Task<bool> ExistsAsync(int id);

        Task<(List<Vehicle> Items, int TotalCount)> GetWithPaginationAsync(
            string? searchModel, string? searchVersion, int? filterStatus, string? filterColor,
            int page, int pageSize);

        Task<List<Vehicle>> SearchAsync(string? searchTerm);

        Task<bool> CheckDuplicateAsync(string model, string version, string color, int? excludeId = null);
        Task<List<string>> GetDistinctModelsAsync();
        Task<List<string>> GetDistinctColorsAsync();

        Task<List<Vehicle>> GetByStatusAsync(int status);
        Task<Inventory?> GetInventoryByVehicleAsync(int vehicleId);
        Task<int> GetTotalCountAsync();
        Task<int> GetCountByStatusAsync(int status);
        Task<bool> HasRelatedDataAsync(int vehicleId);

        // Sync methods
        Inventory? GetInventoryByVehicle(int vehicleId);
        void UpdateInventory(Inventory inventory);

        // Async inventory update
        Task UpdateInventoryAsync(Inventory inventory);
        Task<Vehicle?> GetByIdWithRelaAsync(int id);
    }
}
