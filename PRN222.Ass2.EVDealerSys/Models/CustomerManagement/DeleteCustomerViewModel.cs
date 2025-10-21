namespace PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

public class DeleteCustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DealerName { get; set; } = string.Empty;
    public int OrderCount { get; set; }

    public bool HasOrders => OrderCount > 0;

    public string GetWarningMessage()
    {
        if (HasOrders)
        {
            return $"Khách hàng này có {OrderCount} đơn hàng. Xóa khách hàng sẽ ảnh hưởng đến các đơn hàng này.";
        }
        return string.Empty;
    }
}
