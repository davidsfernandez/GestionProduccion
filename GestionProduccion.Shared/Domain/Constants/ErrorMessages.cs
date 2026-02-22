namespace GestionProduccion.Domain.Constants;

public static class ErrorMessages
{
    public const string ElementNotFound = "Elemento no encontrado";
    public const string DuplicateCode = "Código duplicado";
    public const string CannotDeleteByBusinessRules = "No se puede eliminar por reglas de negocio";
    public const string OrderAlreadyInProgress = "La orden ya está en progreso y no puede eliminarse";
}
