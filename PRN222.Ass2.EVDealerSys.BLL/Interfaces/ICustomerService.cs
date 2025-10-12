using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Customer;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

namespace PRN222.Ass2.EVDealerSys.BLL.Interfaces;
public interface ICustomerService
{
    Task<(IEnumerable<Customer> customers, int totalCount, int totalPages)> GetPagedCustomersAsync(
            int page, int pageSize, string? searchName = null, string? searchEmail = null,
            string? searchPhone = null, int? dealerId = null);

    Task<List<CustomerItemDto>> SearchAsync(string? searchTerm, int? dealerId = null);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto?> GetByPhoneAsync(string phone);

    Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null);
    Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);

    Task<Customer?> GetCustomerByIdAsync(int id);

    Task<Customer> CreateCustomerAsync(string name, string email, string phone, string address, int dealerId);
    Task<Customer> UpdateCustomerAsync(int id, string name, string email, string phone, string address, int dealerId);
    Task<bool> DeleteCustomerAsync(int id);
    Task<(bool Success, string Message, Customer? Customer)> CreateAsync(CustomerDto viewModel);

    Task<(bool Success, string Message, Customer? Customer)> UpdateAsync(CustomerDto viewModel);

    Task<bool> DeleteAsync(int id);

    Task<bool> CustomerExistsAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    Task<int> GetTotalCustomersCountAsync();
    Task<int> GetCustomersWithOrdersCountAsync();
    Task<int> GetTotalOrdersCountAsync();
    Task<decimal> GetTotalOrderValueAsync();
    Task<int> GetNewCustomersThisMonthCountAsync();

    IEnumerable<Customer> GetAllCustomers();

    Task<IEnumerable<Customer>> GetAllCustomersAsync();
    Task<int> GetCustomersCountByDateRangeAsync(DateTime startDate, DateTime endDate, int? dealerId = null);
}