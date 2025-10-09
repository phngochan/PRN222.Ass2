using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Interfaces
{
    public interface IDealerRepository : IGenericRepository<Dealer>
    {
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);

        Task<IEnumerable<Dealer>> SearchAsync(string? name, string? region, string? address = null);

        Task<int> GetTotalCountAsync();
        Task<int> GetDealersWithCustomersCountAsync();
        Task<int> GetDealersWithInventoryCountAsync();

        Task<IEnumerable<Dealer>> GetPagedAsync(int page, int pageSize, string? name, string? region, string? address = null);
        Task<int> GetTotalPagesAsync(int pageSize, string? name, string? region, string? address = null);

        Task<IEnumerable<Dealer>> GetByRegionAsync(string region);
        Task<IEnumerable<string>> GetDistinctRegionsOnlyAsync();
        Task<IEnumerable<Dealer>> GetDealersWithBasicInfoAsync();
        Task<List<string>> GetRegionsAsync();
    }
}
