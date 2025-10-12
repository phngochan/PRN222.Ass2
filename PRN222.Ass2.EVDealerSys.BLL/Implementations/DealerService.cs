using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class DealerService(IDealerRepository dealerRepo) : IDealerService
{
    private readonly IDealerRepository _dealerRepo = dealerRepo;

    public async Task<IEnumerable<Dealer>> GetAllDealersAsync()
    {
        return await _dealerRepo.GetAllAsync();
    }

    public async Task<(IEnumerable<Dealer> dealers, int totalCount, int totalPages)> GetPagedDealersAsync(
        int page, int pageSize, string? searchName = null, string? searchRegion = null, string? searchAddress = null)
    {
        var dealers = await _dealerRepo.GetPagedAsync(page, pageSize, searchName, searchRegion, searchAddress);
        var totalPages = await _dealerRepo.GetTotalPagesAsync(pageSize, searchName, searchRegion, searchAddress);
        var totalCount = await GetFilteredDealersCountAsync(searchName, searchRegion, searchAddress);

        return (dealers, totalCount, totalPages);
    }

    public async Task<Dealer?> GetDealerByIdAsync(int id)
    {
        return await _dealerRepo.GetByIdAsync(id);
    }

    public async Task<Dealer> CreateDealerAsync(string name, string address, string region)
    {
        var dealer = new Dealer
        {
            Name = name,
            Address = address,
            Region = region
        };

        return await _dealerRepo.CreateAsync(dealer);
    }

    public async Task<Dealer> UpdateDealerAsync(int id, string name, string address, string region)
    {
        var dealer = await _dealerRepo.GetByIdAsync(id);
        if (dealer == null)
            throw new ArgumentException("Dealer not found");

        dealer.Name = name;
        dealer.Address = address;
        dealer.Region = region;

        return await _dealerRepo.UpdateAsync(dealer);
    }

    public async Task<bool> DeleteDealerAsync(int id)
    {
        return await _dealerRepo.DeleteAsync(id);
    }

    public async Task<bool> DealerExistsAsync(int id)
    {
        return await _dealerRepo.ExistsAsync(id);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _dealerRepo.NameExistsAsync(name, excludeId);
    }

    public async Task<int> GetTotalDealersCountAsync()
    {
        return await _dealerRepo.GetTotalCountAsync();
    }

    public async Task<int> GetDealersWithCustomersCountAsync()
    {
        return await _dealerRepo.GetDealersWithCustomersCountAsync();
    }

    public async Task<int> GetDealersWithInventoryCountAsync()
    {
        return await _dealerRepo.GetDealersWithInventoryCountAsync();
    }

    public async Task<int> GetTotalCustomersCountAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.SelectMany(d => d.Customers).Count();
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.SelectMany(d => d.Orders).Count();
    }

    public async Task<IEnumerable<Dealer>> GetDealersByRegionAsync(string region)
    {
        return await _dealerRepo.GetByRegionAsync(region);
    }

    public async Task<IEnumerable<string>> GetDistinctRegionsAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.Where(d => !string.IsNullOrEmpty(d.Region))
                     .Select(d => d.Region!)
                     .Distinct()
                     .OrderBy(r => r)
                     .ToList();
    }

    public async Task<IEnumerable<Dealer>> GetActiveDealersAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.Where(d => !string.IsNullOrEmpty(d.Name))
                     .OrderBy(d => d.Name)
                     .ToList();
    }

    public async Task<IEnumerable<Dealer>> GetDealersForSelectionAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.Where(d => !string.IsNullOrEmpty(d.Name) && !string.IsNullOrEmpty(d.Region))
                     .OrderBy(d => d.Region)
                     .ThenBy(d => d.Name)
                     .ToList();
    }

    public async Task<Dictionary<string, List<Dealer>>> GetDealersGroupedByRegionAsync()
    {
        var dealers = await _dealerRepo.GetAllAsync();
        return dealers.Where(d => !string.IsNullOrEmpty(d.Region) && !string.IsNullOrEmpty(d.Name))
                     .GroupBy(d => d.Region!)
                     .ToDictionary(
                         g => g.Key,
                         g => g.OrderBy(d => d.Name).ToList()
                     );
    }

    private async Task<int> GetFilteredDealersCountAsync(string? name, string? region, string? address)
    {
        var dealers = await _dealerRepo.SearchAsync(name, region, address);
        return dealers.Count();
    }
}
