using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface IDealerService
{
    Task<IEnumerable<Dealer>> GetAllDealersAsync();

    Task<(IEnumerable<Dealer> dealers, int totalCount, int totalPages)> GetPagedDealersAsync(
        int page, int pageSize, string? searchName = null, string? searchRegion = null, string? searchAddress = null);

    Task<Dealer?> GetDealerByIdAsync(int id);

    Task<Dealer> CreateDealerAsync(string name, string address, string region);
    Task<Dealer> UpdateDealerAsync(int id, string name, string address, string region);
    Task<bool> DeleteDealerAsync(int id);

    Task<bool> DealerExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    Task<int> GetTotalDealersCountAsync();
    Task<int> GetDealersWithCustomersCountAsync();
    Task<int> GetDealersWithInventoryCountAsync();
    Task<int> GetTotalCustomersCountAsync();
    Task<int> GetTotalOrdersCountAsync();

    Task<IEnumerable<Dealer>> GetDealersByRegionAsync(string region);
    Task<IEnumerable<string>> GetDistinctRegionsAsync();
    Task<IEnumerable<Dealer>> GetActiveDealersAsync();
    Task<IEnumerable<Dealer>> GetDealersForSelectionAsync();
    Task<Dictionary<string, List<Dealer>>> GetDealersGroupedByRegionAsync();
}
