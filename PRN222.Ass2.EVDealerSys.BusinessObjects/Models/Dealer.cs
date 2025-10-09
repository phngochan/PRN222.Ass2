namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Dealer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? Region { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<VehicleAllocation> VehicleAllocations { get; set; } = new List<VehicleAllocation>();
}
