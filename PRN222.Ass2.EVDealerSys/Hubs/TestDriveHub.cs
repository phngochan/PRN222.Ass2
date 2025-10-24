using Microsoft.AspNetCore.SignalR;

namespace PRN222.Ass2.EVDealerSys.Hubs;

/// <summary>
/// SignalR Hub for real-time Test Drive notifications
/// Provides real-time updates for test drive bookings, status changes, and dashboard updates
/// </summary>
public class TestDriveHub : Hub
{
    private readonly ILogger<TestDriveHub> _logger;

    public TestDriveHub(ILogger<TestDriveHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected to TestDriveHub: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected from TestDriveHub: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific dealer group to receive notifications for that dealer only
    /// </summary>
    public async Task JoinDealerGroup(int dealerId)
    {
        var groupName = $"Dealer_{dealerId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined dealer group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Leave a specific dealer group
    /// </summary>
    public async Task LeaveDealerGroup(int dealerId)
    {
        var groupName = $"Dealer_{dealerId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left dealer group {GroupName}", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Notify all clients about a new test drive booking
    /// </summary>
    public async Task NotifyTestDriveCreated(object testDrive)
    {
        await Clients.All.SendAsync("TestDriveCreated", testDrive);
    }

    /// <summary>
    /// Notify all clients about a test drive update
    /// </summary>
    public async Task NotifyTestDriveUpdated(object testDrive)
    {
        await Clients.All.SendAsync("TestDriveUpdated", testDrive);
    }

    /// <summary>
    /// Notify all clients about a test drive status change
    /// </summary>
    public async Task NotifyTestDriveStatusChanged(int testDriveId, int oldStatus, int newStatus, string statusName)
    {
        await Clients.All.SendAsync("TestDriveStatusChanged", new
        {
            testDriveId,
            oldStatus,
            newStatus,
            statusName,
            timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// Notify specific dealer group about a test drive event
    /// </summary>
    public async Task NotifyDealerGroup(int dealerId, string eventType, object data)
    {
        var groupName = $"Dealer_{dealerId}";
        await Clients.Group(groupName).SendAsync(eventType, data);
    }
}
