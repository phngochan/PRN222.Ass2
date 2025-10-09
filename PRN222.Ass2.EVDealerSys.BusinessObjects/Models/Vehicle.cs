using System.ComponentModel.DataAnnotations.Schema;

namespace PRN222.Ass2.EVDealerSys.BusinessObjects.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public string? Model { get; set; }

    public string? Version { get; set; }

    public string? Color { get; set; }

    public string? Config { get; set; }

    public decimal? Price { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<VehicleAllocation> VehicleAllocations { get; set; } = new List<VehicleAllocation>();

  
}


