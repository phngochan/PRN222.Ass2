using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class VehicleService(IVehicleRepository vehicleRepository) : IVehicleService
{
    private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
    public async Task<(List<Vehicle> Vehicles, int TotalCount)> GetVehiclesWithPaginationAsync(
            string? searchModel, string? searchVersion, int? filterStatus, string? filterColor,
            int page, int pageSize)
    {
        return await _vehicleRepository.GetWithPaginationAsync(
            searchModel, searchVersion, filterStatus, filterColor, page, pageSize);
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return await _vehicleRepository.GetByIdWithRelaAsync(id);
    }

    public async Task<bool> IsVehicleExistsAsync(string model, string version, string color, int? excludeId = null)
    {
        return await _vehicleRepository.CheckDuplicateAsync(model, version, color, excludeId);
    }

    public async Task<Vehicle> CreateVehicleAsync(Vehicle vehicle)
    {
        var exists = await _vehicleRepository.CheckDuplicateAsync(
            vehicle.Model, vehicle.Version, vehicle.Color);

        if (exists)
        {
            throw new InvalidOperationException("Xe với model, phiên bản và màu sắc này đã tồn tại.");
        }

        return await _vehicleRepository.AddAsync(vehicle);
    }

    public async Task<Vehicle> UpdateVehicleAsync(Vehicle vehicle)
    {
        // Business logic: check exists
        var existingVehicle = await _vehicleRepository.GetByIdWithRelaAsync(vehicle.Id);
        if (existingVehicle == null)
        {
            throw new InvalidOperationException("Không tìm thấy xe để cập nhật.");
        }

        // Business logic: check duplicates
        var isDuplicate = await _vehicleRepository.CheckDuplicateAsync(
            vehicle.Model, vehicle.Version, vehicle.Color, vehicle.Id);

        if (isDuplicate)
        {
            throw new InvalidOperationException("Xe với model, phiên bản và màu sắc này đã tồn tại.");
        }

        // Update properties
        existingVehicle.Model = vehicle.Model;
        existingVehicle.Version = vehicle.Version;
        existingVehicle.Color = vehicle.Color;
        existingVehicle.Config = vehicle.Config;
        existingVehicle.Price = vehicle.Price;
        existingVehicle.Status = vehicle.Status;

        return await _vehicleRepository.UpdateAsync(existingVehicle);
    }

    public async Task UpdateVehicleStatusAsync(int id, int status)
    {
        var vehicle = await _vehicleRepository.GetByIdWithRelaAsync(id);
        if (vehicle == null)
        {
            throw new InvalidOperationException("Không tìm thấy xe.");
        }

        // Business logic: validate status
        if (status < 1 || status > 4)
        {
            throw new ArgumentException("Trạng thái không hợp lệ.");
        }

        vehicle.Status = status;
        await _vehicleRepository.UpdateAsync(vehicle);
    }

    public async Task<bool> CanDeleteVehicleAsync(int id)
    {
        return !await _vehicleRepository.HasRelatedDataAsync(id);
    }

    public async Task DeleteVehicleAsync(int id)
    {
        // Business logic: check if can delete
        var canDelete = await CanDeleteVehicleAsync(id);
        if (!canDelete)
        {
            throw new InvalidOperationException("Không thể xóa xe này vì đã có dữ liệu liên quan (inventory, orders, allocations).");
        }

        var vehicle = await _vehicleRepository.GetByIdWithRelaAsync(id);
        if (vehicle == null)
        {
            throw new InvalidOperationException("Không tìm thấy xe để xóa.");
        }

        await _vehicleRepository.DeleteAsync(id);
    }

    public async Task<VehicleStatisticsDto> GetVehicleStatisticsAsync()
    {
        return new VehicleStatisticsDto
        {
            TotalVehicles = await _vehicleRepository.GetTotalCountAsync(),
            AvailableVehicles = await _vehicleRepository.GetCountByStatusAsync(1),
            SoldVehicles = await _vehicleRepository.GetCountByStatusAsync(2),
            MaintenanceVehicles = await _vehicleRepository.GetCountByStatusAsync(3)
        };
    }

    public async Task<List<string>> GetDistinctModelsAsync()
    {
        return await _vehicleRepository.GetDistinctModelsAsync();
    }

    public async Task<List<string>> GetDistinctColorsAsync()
    {
        return await _vehicleRepository.GetDistinctColorsAsync();
    }
    public IEnumerable<Vehicle> GetAllVehicles()
    {
        return _vehicleRepository.GetAll();
    }

    public Vehicle? GetVehicleById(int id)
    {
        return _vehicleRepository.GetById(id);
    }

    public Inventory? GetInventoryByVehicle(int vehicleId)
    {
        return _vehicleRepository.GetInventoryByVehicle(vehicleId);
    }

    public void UpdateInventory(Inventory inventory)
    {
        _vehicleRepository.UpdateInventory(inventory);
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
    {
        return await _vehicleRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Vehicle>> GetAllVehiclesActiveAsync()
    {
        return await _vehicleRepository.GetAllActiveAsync();
    }

    public async Task<int> GetTotalVehiclesCountAsync()
    {
        return await _vehicleRepository.GetTotalCountAsync();
    }
}
