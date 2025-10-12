using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Interfaces
{
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        // Query Methods
        Inventory? GetEvmStock(int vehicleId);
        Inventory? GetByVehicle(int vehicleId, int locationType = 1, int? dealerId = null);

        // Stock Operations
        void ReduceEvmStock(int vehicleId, int qty);
        void AddDealerStock(int vehicleId, int dealerId, int qty);
    }
}
