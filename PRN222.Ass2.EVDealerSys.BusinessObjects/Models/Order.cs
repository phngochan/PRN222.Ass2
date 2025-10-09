namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? DealerId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? Status { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Customer? Customer { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
