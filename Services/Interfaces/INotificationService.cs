namespace GestionProduccion.Services.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Notifies all clients about a production order update.
    /// </summary>
    Task NotifyOrderUpdateAsync(int orderId, string stage, string status, CancellationToken ct = default);

    /// <summary>
    /// Sends a general notification to a specific user.
    /// </summary>
    Task NotifyUserAsync(int userId, string title, string message, CancellationToken ct = default);

    /// <summary>
    /// Notifies administrators about a new lead.
    /// </summary>
    Task NotifyNewLeadAsync(string clientName, string email, CancellationToken ct = default);
}
