namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public decimal? Amount { get; set; }

    public int? PaymentMethod { get; set; }

    public DateTime? PaidAt { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }
}
