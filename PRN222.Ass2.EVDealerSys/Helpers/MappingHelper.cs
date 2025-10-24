using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.Models;
using PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

namespace PRN222.Ass2.EVDealerSys.Helpers;

public static class MappingHelper
{
    public static List<CustomerListItem> ToCustomerListItems(IEnumerable<Customer> customers)
    {
        return customers.Select(c => new CustomerListItem
        {
            Id = c.Id,
            Name = c.Name ?? "",
            Email = c.Email ?? "",
            Phone = c.Phone ?? "",
            Address = c.Address ?? "",
            DealerName = c.Dealer?.Name ?? "",
            DealerRegion = c.Dealer?.Region ?? "",
            OrderCount = c.Orders?.Count ?? 0
        }).ToList();
    }

    public static List<DealerSelectItem> ToDealerSelectItems(IEnumerable<Dealer> dealers)
    {
        return dealers.Select(d => new DealerSelectItem
        {
            Id = d.Id,
            Name = d.Name ?? "",
            Region = d.Region ?? ""
        }).ToList();
    }
    
    public static List<DealerListItem> ToDealerListItem(IEnumerable<Dealer> dealers)
    {
        return dealers.Select(d => new DealerListItem
        {
            Id = d.Id,
            Name = d.Name ?? "",
            Address = d.Address ?? "",
            Region = d.Region ?? "",
            CustomerCount = d.Customers?.Count ?? 0,
            InventoryCount = d.Inventories?.Count ?? 0,
            OrderCount = d.Orders?.Count ?? 0
        }).ToList();
    }

    public static DealerDetailsViewModel MapToDealerDetailsViewModel(Dealer dealer)
    {
        return new DealerDetailsViewModel
        {
            Id = dealer.Id,
            Name = dealer.Name ?? "",
            Address = dealer.Address ?? "",
            Region = dealer.Region ?? "",
            TotalCustomers = dealer.Customers?.Count ?? 0,
            TotalInventories = dealer.Inventories?.Count ?? 0,
            TotalOrders = dealer.Orders?.Count ?? 0,
            TotalUsers = dealer.Users?.Count ?? 0,
            TotalAllocations = dealer.VehicleAllocations?.Count ?? 0,
            TotalRevenue = dealer.Orders?.Sum(o => o.TotalPrice ?? 0) ?? 0,
            TopCustomers = dealer.Customers?.Take(5).Select(c => new DealerCustomerInfo
            {
                CustomerId = c.Id,
                CustomerName = c.Name ?? "",
                Email = c.Email ?? "",
                OrderCount = c.Orders?.Count ?? 0
            }).ToList() ?? new List<DealerCustomerInfo>(),
            RecentInventories = dealer.Inventories?.Take(5).Select(i => new DealerInventoryInfo
            {
                InventoryId = i.Id,
                VehicleModel = i.Vehicle?.Model ?? "",
                Quantity = i.Quantity ?? 0,
                LastUpdated = DateTime.Now
            }).ToList() ?? new List<DealerInventoryInfo>()
        };
    }
    
    public static string GetOrderStatusText(int status)
    {
        return status switch
        {
            0 => "Chờ xử lý",
            1 => "Đã xác nhận",
            2 => "Đang giao hàng",
            3 => "Đã hoàn thành",
            4 => "Đã hủy",
            _ => "Không xác định"
        };
    }

    // NEW: Thêm helper cho AllocationStatus
    public static string GetAllocationStatusText(int status)
    {
        return status switch
        {
            0 => "Chờ Manager xét duyệt",
            1 => "Chờ EVM phê duyệt",
            2 => "Đã phê duyệt",
            3 => "Từ chối",
            4 => "Đang vận chuyển",
            5 => "Đã giao",
            6 => "Đã hủy",
            _ => "Không xác định"
        };
    }

    public static string GetAllocationStatusBadgeClass(int status)
    {
        return status switch
        {
            0 => "warning",
            1 => "info",
            2 => "success",
            3 => "danger",
            4 => "primary",
            5 => "success",
            6 => "secondary",
            _ => "secondary"
        };
    }
}
