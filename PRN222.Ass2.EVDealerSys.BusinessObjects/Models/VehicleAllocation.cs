namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class VehicleAllocation
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? FromLocationType { get; set; }

    public int? ToDealerId { get; set; }

    public int? Quantity { get; set; }

    public DateTime? RequestDate { get; set; }

    public DateTime? AllocationDate { get; set; }

    public int? Status { get; set; }

    public virtual Dealer? ToDealer { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
