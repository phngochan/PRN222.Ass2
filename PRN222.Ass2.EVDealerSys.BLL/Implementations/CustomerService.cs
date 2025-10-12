using PRN222.Ass2.EVDealerSys.BLL.Interfaces;
using PRN222.Ass2.EVDealerSys.BusinessObjects.DTO.Customer;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;

public class CustomerService(ICustomerRepository customerRepo) : ICustomerService
{
    private readonly ICustomerRepository _customerRepository = customerRepo;

    public async Task<(IEnumerable<Customer> customers, int totalCount, int totalPages)> GetPagedCustomersAsync(
            int page, int pageSize, string? searchName = null, string? searchEmail = null,
            string? searchPhone = null, int? dealerId = null)
    {
        var customers = await _customerRepository.GetPagedAsync(page, pageSize, searchName, searchEmail, searchPhone, dealerId);
        var totalPages = await _customerRepository.GetTotalPagesAsync(pageSize, searchName, searchEmail, searchPhone, dealerId);
        var totalCount = await GetFilteredCustomersCountAsync(searchName, searchEmail, searchPhone, dealerId);

        return (customers, totalCount, totalPages);
    }
    public async Task<List<CustomerItemDto>> SearchAsync(string? searchTerm, int? dealerId = null)
    {
        var customers = await _customerRepository.SearchAsync(searchTerm, dealerId);
        return customers.Select(MapToItemDto).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<CustomerDto?> GetByPhoneAsync(string phone)
    {
        var customer = await _customerRepository.GetByPhoneAsync(phone);
        return customer == null ? null : MapToDto(customer);
    }

    public async Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null)
    {
        return await _customerRepository.IsPhoneExistsAsync(phone, excludeId);
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
    {
        return await _customerRepository.EmailExistsAsync(email, excludeId);
    }
    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<Customer> CreateCustomerAsync(string name, string email, string phone, string address, int dealerId)
    {
        var customer = new Customer
        {
            Name = name,
            Email = email,
            Phone = phone,
            Address = address,
            DealerId = dealerId
        };

        return await _customerRepository.CreateAsync(customer);
    }

    public async Task<Customer> UpdateCustomerAsync(int id, string name, string email, string phone, string address, int dealerId)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            throw new ArgumentException("Customer not found");

        customer.Name = name;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = address;
        customer.DealerId = dealerId;

        return await _customerRepository.UpdateAsync(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await _customerRepository.DeleteAsync(id);
    }

    public async Task<(bool Success, string Message, Customer? Customer)> CreateAsync(CustomerDto dto)
    {
        try
        {
            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Tên khách hàng không được để trống", null);

            if (string.IsNullOrWhiteSpace(dto.Phone))
                return (false, "Số điện thoại không được để trống", null);

            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "Email không được để trống", null);

            // Kiểm tra số điện thoại đã tồn tại
            if (await IsPhoneExistsAsync(dto.Phone))
                return (false, $"Số điện thoại {dto.Phone} đã tồn tại trong hệ thống", null);

            // Kiểm tra email đã tồn tại
            if (await IsEmailExistsAsync(dto.Email))
                return (false, $"Email {dto.Email} đã tồn tại trong hệ thống", null);

            var customer = new Customer
            {
                Name = dto.Name.Trim(),
                Phone = dto.Phone.Trim(),
                Email = dto.Email.Trim(),
                Address = dto.Address?.Trim(),
                DealerId = dto.DealerId
            };

            var createdCustomer = await _customerRepository.CreateAsync(customer);
            return (true, "Tạo khách hàng thành công", createdCustomer);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<(bool Success, string Message, Customer? Customer)> UpdateAsync(CustomerDto dto)
    {
        try
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(dto.Id);
            if (existingCustomer == null)
                return (false, "Không tìm thấy khách hàng", null);

            // Kiểm tra các trường bắt buộc
            if (string.IsNullOrWhiteSpace(dto.Name))
                return (false, "Tên khách hàng không được để trống", null);

            if (string.IsNullOrWhiteSpace(dto.Phone))
                return (false, "Số điện thoại không được để trống", null);

            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "Email không được để trống", null);

            // Kiểm tra số điện thoại trùng (ngoại trừ khách hàng hiện tại)
            if (await IsPhoneExistsAsync(dto.Phone, dto.Id))
                return (false, $"Số điện thoại {dto.Phone} đã tồn tại trong hệ thống", null);

            // Kiểm tra email trùng (ngoại trừ khách hàng hiện tại)
            if (await IsEmailExistsAsync(dto.Email, dto.Id))
                return (false, $"Email {dto.Email} đã tồn tại trong hệ thống", null);

            existingCustomer.Name = dto.Name.Trim();
            existingCustomer.Phone = dto.Phone.Trim();
            existingCustomer.Email = dto.Email.Trim();
            existingCustomer.Address = dto.Address?.Trim();
            existingCustomer.DealerId = dto.DealerId;

            var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
            return (true, "Cập nhật khách hàng thành công", updatedCustomer);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _customerRepository.DeleteAsync(id);
    }
    public async Task<bool> CustomerExistsAsync(int id)
    {
        return await _customerRepository.ExistsAsync(id);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _customerRepository.EmailExistsAsync(email, excludeId);
    }

    public async Task<int> GetTotalCustomersCountAsync()
    {
        return await _customerRepository.GetTotalCountAsync();
    }

    public async Task<int> GetCustomersWithOrdersCountAsync()
    {
        return await _customerRepository.GetCustomersWithOrdersCountAsync();
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.SelectMany(c => c.Orders).Count();
    }

    private CustomerItemDto MapToItemDto(Customer customer)
    {
        return new CustomerItemDto
        {
            Id = customer.Id,
            Name = customer.Name ?? "",
            Phone = customer.Phone ?? "",
            Email = customer.Email ?? "",
            Address = customer.Address,
            DealerId = customer.DealerId,
            DealerName = customer.Dealer?.Name
        };
    }

    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name ?? "",
            Phone = customer.Phone ?? "",
            Email = customer.Email ?? "",
            Address = customer.Address,
            DealerId = customer.DealerId,
            DealerName = customer.Dealer?.Name
        };
    }
    public async Task<decimal> GetTotalOrderValueAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.SelectMany(c => c.Orders).Sum(o => o.TotalPrice ?? 0);
    }

    public async Task<int> GetNewCustomersThisMonthCountAsync()
    {
        return await _customerRepository.GetNewCustomersThisMonthAsync();
    }

    public IEnumerable<Customer> GetAllCustomers()
    {
        return _customerRepository.GetAll();
    }
    private async Task<int> GetFilteredCustomersCountAsync(string? name, string? email, string? phone, int? dealerId)
    {
        var customers = await _customerRepository.SearchAsync(name, email, phone, dealerId);
        return customers.Count();
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }

    public async Task<int> GetCustomersCountByDateRangeAsync(DateTime startDate, DateTime endDate, int? dealerId = null)
    {
        if (dealerId.HasValue)
        {
            return await _customerRepository.GetCountByDateRangeAndDealerAsync(startDate, endDate, dealerId.Value);
        }
        return await _customerRepository.GetCountByDateRangeAsync(startDate, endDate);
    }
}
