namespace GestionProduccion.Domain.Interfaces;

/// <summary>
/// Interface for entities that require creation and update tracking.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
