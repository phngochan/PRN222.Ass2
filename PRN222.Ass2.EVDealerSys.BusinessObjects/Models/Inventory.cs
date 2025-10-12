namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Inventory
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? LocationType { get; set; }

    public int? DealerId { get; set; }

    public int? Quantity { get; set; }

    public virtual Dealer? Dealer { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
