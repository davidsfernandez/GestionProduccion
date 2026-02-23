namespace GestionProduccion.Domain.Exceptions;

public class DomainConstraintException : Exception
{
    public DomainConstraintException(string message) : base(message) { }
    public DomainConstraintException(string message, Exception innerException) : base(message, innerException) { }
}
