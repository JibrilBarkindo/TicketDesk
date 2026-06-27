namespace TicketDesk.Api.Services;

/// <summary>Thrown when a requested resource does not exist. Mapped to HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Thrown when a business rule is violated. Mapped to HTTP 400.</summary>
public class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message) { }
}
