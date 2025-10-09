using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Interfaces
{
    public interface ICustomerRepository: IGenericRepository<Customer>
    {
        Task<List<Customer>> SearchAsync(string? searchTerm, int? dealerId = null);
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<Customer?> GetByEmailAsync(string email);

        Task<bool> ExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null);

        Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? phone, int? dealerId);

        Task<int> GetTotalCountAsync();
        Task<int> GetCustomersWithOrdersCountAsync();
        Task<int> GetNewCustomersThisMonthAsync();

        Task<IEnumerable<Customer>> GetPagedAsync(
            int page, int pageSize, string? name, string? email, string? phone, int? dealerId);

        Task<int> GetTotalPagesAsync(
            int pageSize, string? name, string? email, string? phone, int? dealerId);

        Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetCountByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId);
    }
}
