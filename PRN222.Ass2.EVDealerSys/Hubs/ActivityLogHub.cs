using Microsoft.AspNetCore.SignalR;

namespace PRN222.Ass2.EVDealerSys.Hubs;

public class ActivityLogHub : Hub
{
    public async Task NotifyNewLog(object logData)
    {
        await Clients.All.SendAsync("ReceiveNewLog", logData);
    }

    public async Task NotifyLogUpdate()
    {
        await Clients.All.SendAsync("ReceiveLogUpdate");
    }
}
