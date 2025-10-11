using PRN222.Ass2.EVDealerSys.BusinessObjects.Models;
using PRN222.Ass2.EVDealerSys.DAL.Interfaces;
using PRN222.Ass2.EVDealerSys.DAL.Context;
using PRN222.Ass2.EVDealerSys.DAL.Base;

namespace PRN222.Ass2.EVDealerSys.DAL.Implementations
{
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(EvdealerDbContext context) : base(context)
        {
        }

        public Inventory? GetEvmStock(int vehicleId)
        {
            return _context.Inventories
                .FirstOrDefault(i => i.VehicleId == vehicleId && i.LocationType == 1); // 1 = Hãng (EVM)
        }

        public void ReduceEvmStock(int vehicleId, int qty)
        {
            var stock = GetEvmStock(vehicleId);
            if (stock == null || stock.Quantity < qty)
                throw new Exception("Không đủ tồn kho tại hãng!");

            stock.Quantity -= qty;
            _context.Inventories.Update(stock);
            _context.SaveChanges();
        }

        public void AddDealerStock(int vehicleId, int dealerId, int qty)
        {
            var stock = _context.Inventories
                .FirstOrDefault(i => i.VehicleId == vehicleId && i.DealerId == dealerId && i.LocationType == 2);

            if (stock == null)
            {
                stock = new Inventory
                {
                    VehicleId = vehicleId,
                    DealerId = dealerId,
                    LocationType = 2,
                    Quantity = qty
                };
                _context.Inventories.Add(stock);
            }
            else
            {
                stock.Quantity += qty;
                _context.Inventories.Update(stock);
            }
            _context.SaveChanges();
        }
        
        public Inventory? GetByVehicle(int vehicleId, int locationType = 1, int? dealerId = null)
        {
            return _context.Inventories
                .FirstOrDefault(i => i.VehicleId == vehicleId
                                     && i.LocationType == locationType
                                     && (dealerId == null || i.DealerId == dealerId));
        }
    }
}
