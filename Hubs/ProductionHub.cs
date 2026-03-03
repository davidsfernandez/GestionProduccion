using Microsoft.AspNetCore.SignalR;

namespace GestionProduccion.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications about changes in Production Orders.
/// </summary>
public class ProductionHub : Hub
{
    public async Task JoinTeamGroup(int teamId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Team_{teamId}");
    }

    public async Task LeaveTeamGroup(int teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Team_{teamId}");
    }

    public async Task JoinDashboardGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Dashboards");
    }

    public async Task NotifyUpdate(int orderId, string newStage, string newStatus, int? teamId = null)
    {
        // Notify the specific team involved
        if (teamId.HasValue)
        {
            await Clients.Group($"Team_{teamId.Value}").SendAsync("ReceiveUpdate", orderId, newStage, newStatus);
        }

        // Always notify Dashboards and TV screens
        await Clients.Group("Dashboards").SendAsync("ReceiveUpdate", orderId, newStage, newStatus);
    }

    /// <summary>
    /// Method that the frontend can use to notify changes (optional).
    /// </summary>
    public async Task SendUpdate(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            message = message,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Connection event: notifies when a user connects.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("UserConnected", new
        {
            userId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Disconnection event: notifies when a user disconnects.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", new
        {
            userId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
        await base.OnDisconnectedAsync(exception);
    }
}