using PRN222.Ass2.EVDealerSys.DAL.Interfaces;

namespace PRN222.Ass2.EVDealerSys.BLL.Implementations;
public class InventoryService(IInventoryRepository repo)
{
    private readonly IInventoryRepository _repo = repo;

    public bool CheckStock(int vehicleId, int qty)
    {
        var stock = _repo.GetEvmStock(vehicleId);
        return stock != null && stock.Quantity >= qty;
    }

    public void AllocateVehicle(int vehicleId, int dealerId, int qty)
    {
        _repo.ReduceEvmStock(vehicleId, qty);
        _repo.AddDealerStock(vehicleId, dealerId, qty);
    }
}
