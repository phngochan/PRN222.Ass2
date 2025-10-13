using Microsoft.AspNetCore.SignalR;

using System.Threading.Tasks;

namespace PRN222.Ass2.EVDealerSys.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NotifyNewOrder(object orderData)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", orderData);
        }
    }
}
