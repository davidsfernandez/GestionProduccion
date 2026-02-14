namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Defines the current state of a production order at a given time.
/// </summary>
public enum ProductionStatus
{
    InProduction,
    Stopped,
    Completed,
    Paused,
    Finished,
    Cancelled
}
