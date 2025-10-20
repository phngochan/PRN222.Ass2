using Microsoft.AspNetCore.SignalR;

namespace PRN222.Ass2.EVDealerSys.Hubs;

public class ManagementHub : Hub
{
    // Dealer notifications
    public async Task NotifyDealerCreated(object dealerData)
    {
        await Clients.All.SendAsync("ReceiveDealerCreated", dealerData);
    }

    public async Task NotifyDealerUpdated(object dealerData)
    {
        await Clients.All.SendAsync("ReceiveDealerUpdated", dealerData);
    }

    public async Task NotifyDealerDeleted(int dealerId, string dealerName)
    {
        await Clients.All.SendAsync("ReceiveDealerDeleted", new { id = dealerId, name = dealerName });
    }

    // Customer notifications
    public async Task NotifyCustomerCreated(object customerData)
    {
        await Clients.All.SendAsync("ReceiveCustomerCreated", customerData);
    }

    public async Task NotifyCustomerUpdated(object customerData)
    {
        await Clients.All.SendAsync("ReceiveCustomerUpdated", customerData);
    }

    public async Task NotifyCustomerDeleted(int customerId, string customerName)
    {
        await Clients.All.SendAsync("ReceiveCustomerDeleted", new { id = customerId, name = customerName });
    }

    // General notification
    public async Task SendNotification(string message, string type = "info")
    {
        await Clients.All.SendAsync("ReceiveNotification", new { message, type });
    }
}
