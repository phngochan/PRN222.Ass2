namespace PRN222.Ass2.EVDealerSys.Models.CustomerManagement;

public class CustomerOrderInfo
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}
