using Microsoft.EntityFrameworkCore;
using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Repositories.Interfaces;
using PRN222.Ass2.EVDealerSys.Repositories.Context;
using PRN222.Ass2.EVDealerSys.Repositories.Base;

namespace PRN222.Ass2.EVDealerSys.Repositories.Implementations
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(EvdealerDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.User)
                        .ThenInclude(u => u.Dealer)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Vehicle)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Dealer)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Customer>> SearchAsync(string? searchTerm, int? dealerId = null)
        {
            var query = _context.Customers
                .Include(c => c.Dealer)
                .AsQueryable();

            if (dealerId.HasValue)
            {
                query = query.Where(c => c.DealerId == dealerId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(term)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)));
            }

            return await query
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            return await _context.Customers
                .Include(c => c.Dealer)
                .FirstOrDefaultAsync(c => c.Phone == phone.Trim());
        }
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id);
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _context.Customers
                .Include(c => c.Dealer)
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == email.ToLower().Trim());
        }
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Customers.Where(c => c.Email == email);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            var query = _context.Customers
                .Where(c => c.Phone == phone.Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, string? phone, int? dealerId)
        {
            var query = _context.Customers.Include(c => c.Dealer).Include(c => c.Orders).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name!.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email!.Contains(email));

            if (!string.IsNullOrEmpty(phone))
                query = query.Where(c => c.Phone!.Contains(phone));

            if (dealerId.HasValue)
                query = query.Where(c => c.DealerId == dealerId.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var query = _context.Customers
                .Where(c => c.Email != null && c.Email.ToLower() == email.ToLower().Trim());

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<int> GetCustomersWithOrdersCountAsync()
        {
            return await _context.Customers
                .Where(c => c.Orders.Any())
                .CountAsync();
        }

        public async Task<int> GetNewCustomersThisMonthAsync()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            return await _context.Customers
                .Where(c => c.Orders.Any(o => o.OrderDate >= startOfMonth))
                .CountAsync();
        }
        public override async Task<Customer> CreateAsync(Customer customer)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new ArgumentException("Tên khách hàng không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Phone))
                throw new ArgumentException("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new ArgumentException("Email không được để trống");

            // Check for duplicate phone
            var existingByPhone = await GetByPhoneAsync(customer.Phone);
            if (existingByPhone != null)
                throw new ArgumentException($"Số điện thoại {customer.Phone} đã tồn tại trong hệ thống");

            // Check for duplicate email
            var existingByEmail = await GetByEmailAsync(customer.Email);
            if (existingByEmail != null)
                throw new ArgumentException($"Email {customer.Email} đã tồn tại trong hệ thống");

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public override async Task<Customer> UpdateAsync(Customer customer)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new ArgumentException("Tên khách hàng không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Phone))
                throw new ArgumentException("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(customer.Email))
                throw new ArgumentException("Email không được để trống");

            // Check for duplicate phone (exclude current customer)
            if (await IsPhoneExistsAsync(customer.Phone, customer.Id))
                throw new ArgumentException($"Số điện thoại {customer.Phone} đã tồn tại trong hệ thống");

            // Check for duplicate email (exclude current customer)
            if (await IsEmailExistsAsync(customer.Email, customer.Id))
                throw new ArgumentException($"Email {customer.Email} đã tồn tại trong hệ thống");

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer == null) return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> GetPagedAsync(int page, int pageSize, string? name, string? email, string? phone, int? dealerId)
        {
            var query = _context.Customers.Include(c => c.Dealer).Include(c => c.Orders).AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name!.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email!.Contains(email));

            if (!string.IsNullOrEmpty(phone))
                query = query.Where(c => c.Phone!.Contains(phone));

            if (dealerId.HasValue)
                query = query.Where(c => c.DealerId == dealerId.Value);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalPagesAsync(int pageSize, string? name, string? email, string? phone, int? dealerId)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name!.Contains(name));

            if (!string.IsNullOrEmpty(email))
                query = query.Where(c => c.Email!.Contains(email));

            if (!string.IsNullOrEmpty(phone))
                query = query.Where(c => c.Phone!.Contains(phone));

            if (dealerId.HasValue)
                query = query.Where(c => c.DealerId == dealerId.Value);

            var totalItems = await query.CountAsync();
            return (int)Math.Ceiling((double)totalItems / pageSize);
        }

        public async Task<int> GetCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Customers
                .Where(c => c.Orders.Any(o => o.OrderDate >= startDate && o.OrderDate <= endDate))
                .CountAsync();
        }

        public async Task<int> GetCountByDateRangeAndDealerAsync(DateTime startDate, DateTime endDate, int dealerId)
        {
            return await _context.Customers
                .Where(c => c.Orders.Any(o => o.OrderDate >= startDate && o.OrderDate <= endDate
                                             && o.User != null && o.User.DealerId == dealerId))
                .CountAsync();
        }
    }
}
