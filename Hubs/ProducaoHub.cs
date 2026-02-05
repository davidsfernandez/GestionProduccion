using Microsoft.AspNetCore.SignalR;

namespace GestionProduccion.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications about changes in Production Orders.
/// </summary>
public class ProductionHub : Hub
{
    /// <summary>
    /// Notifies all connected clients about an update in a production order.
    /// Called automatically from ProductionOrderService when changes occur.
    /// </summary>
    /// <param name="orderId">ID of the updated Production Order</param>
    /// <param name="newStage">New process stage (Cutting, Sewing, Review, Packaging)</param>
    /// <param name="newStatus">New status (InProduction, Stopped, Completed)</param>
    public async Task NotifyUpdate(int orderId, string newStage, string newStatus)
    {
        await Clients.All.SendAsync("ReceiveUpdate", new
        {
            orderId = orderId,
            stage = newStage,
            status = newStatus,
            timestamp = DateTime.UtcNow
        });
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