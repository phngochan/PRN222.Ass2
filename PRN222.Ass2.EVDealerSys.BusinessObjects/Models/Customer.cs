namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public int? DealerId { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
