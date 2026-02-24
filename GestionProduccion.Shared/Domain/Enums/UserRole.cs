namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Defines user roles in the system.
/// </summary>
public enum UserRole
{
    Administrator = 0, // Role with all permissions
    Leader = 1,        // Team leader role
    Operational = 2,   // Production operator role
    Office = 3         // Administrative/Office role
}
