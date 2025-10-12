namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Contract
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public string? FileUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Order? Order { get; set; }
}
