namespace PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

public class CustomerManagementViewModel
{
    public List<CustomerListItem> Customers { get; set; } = new List<CustomerListItem>();
    public int TotalCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public int TotalOrders { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string? SearchName { get; set; }
    public string? SearchEmail { get; set; }
    public string? SearchPhone { get; set; }
    public int? SelectedDealerId { get; set; }
    public List<DealerSelectItem> AvailableDealers { get; set; } = new List<DealerSelectItem>();
}
