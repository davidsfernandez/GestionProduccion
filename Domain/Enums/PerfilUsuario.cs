namespace GestionProduccion.Domain.Enums;

/// <summary>
/// Define los roles de los usuarios en el sistema.
/// </summary>
public enum PerfilUsuario
{
    Administrador, // Rol con todos los permisos
    Lider,         // Rol de líder de equipo
    Costureira,    // Rol de operario de costura
    Oficina        // Rol que representa un taller o proveedor externo
}
