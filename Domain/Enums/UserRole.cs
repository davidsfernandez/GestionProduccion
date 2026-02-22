namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Defines user roles in the system.
/// </summary>
public enum UserRole
{
    Administrator, // Role with all permissions
    Leader,        // Team leader role
    Operator,      // Production operator role
    Workshop,      // Role representing an external workshop or provider
    Sewer,         // Legacy role (now mapped to Operator in UI)
    Operational    // New role for basic operational tasks
}
