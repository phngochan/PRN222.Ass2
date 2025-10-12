using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IVehicleService
{
    Task<(List<Vehicle> Vehicles, int TotalCount)> GetVehiclesWithPaginationAsync(
        string? searchModel, string? searchVersion, int? filterStatus, string? filterColor,
        int page, int pageSize);

    Task<Vehicle?> GetVehicleByIdAsync(int id);
    Task<bool> IsVehicleExistsAsync(string model, string version, string color, int? excludeId = null);
    Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
    Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle);
    Task UpdateVehicleStatusAsync(int id, int status);

    Task<bool> CanDeleteVehicleAsync(int id);
    Task DeleteVehicleAsync(int id);

    Task<VehicleStatisticsDto> GetVehicleStatisticsAsync();

    Task<List<string>> GetDistinctModelsAsync();
    Task<List<string>> GetDistinctColorsAsync();

    IEnumerable<Vehicle> GetAllVehicles();
    Vehicle? GetVehicleById(int id);

    Inventory? GetInventoryByVehicle(int vehicleId);
    void UpdateInventory(Inventory inventory);

    Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
    Task<IEnumerable<Vehicle>> GetAllVehiclesActiveAsync();
    Task<int> GetTotalVehiclesCountAsync();
}

public class VehicleStatisticsDto
{
    public int TotalVehicles { get; set; }
    public int AvailableVehicles { get; set; }
    public int MaintenanceVehicles { get; set; }
    public int SoldVehicles { get; set; }
}