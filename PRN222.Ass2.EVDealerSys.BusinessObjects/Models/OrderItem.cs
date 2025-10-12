namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? VehicleId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
