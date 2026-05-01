namespace Presupuestos.Application.Common.Exceptions;

public class AuthException : Exception
{
    public AuthException(string message) : base(message) { }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message) { }
}

public class ForbiddenAppException : Exception
{
    public ForbiddenAppException(string message) : base(message) { }
}
