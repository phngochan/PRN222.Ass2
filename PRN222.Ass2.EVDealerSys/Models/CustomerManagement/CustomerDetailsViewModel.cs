namespace PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

public class CustomerDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DealerId { get; set; }
    public string DealerName { get; set; } = string.Empty;
    public string DealerRegion { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public List<CustomerOrderInfo> RecentOrders { get; set; } = new List<CustomerOrderInfo>();
}
