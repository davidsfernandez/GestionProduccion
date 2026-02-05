namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Defines user roles in the system.
/// </summary>
public enum UserRole
{
    Administrator, // Role with all permissions
    Leader,        // Team leader role
    Sewer,         // Sewing operator role
    Workshop       // Role representing an external workshop or provider
}
