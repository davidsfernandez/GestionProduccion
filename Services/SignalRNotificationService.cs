using GestionProduccion.Hubs;
using GestionProduccion.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace GestionProduccion.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<ProductionHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IHubContext<ProductionHub> hubContext, ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderUpdateAsync(int orderId, string stage, string status, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveUpdate", orderId, stage, status, cancellationToken: ct);
            _logger.LogInformation("Order {OrderId} update notification sent: {Stage}, {Status}", orderId, stage, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for order {OrderId}", orderId);
        }
    }

    public async Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default)
    {
        try
        {
            // For now, simple console/log simulation or targeted client notification
            // In a real scenario, we would use _hubContext.Clients.User(userId.ToString())
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", userId, title, message, cancellationToken: ct);
            _logger.LogInformation("User {UserId} notified: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify user {UserId}", userId);
        }
    }

    public async Task NotifyNewLeadAsync(string clientName, string email, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNewLead", new {
                name = clientName,
                email = email,
                timestamp = DateTime.UtcNow
            }, cancellationToken: ct);
            _logger.LogInformation("New lead notification sent for: {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for new lead {Email}", email);
        }
    }
}
