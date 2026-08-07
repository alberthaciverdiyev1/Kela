namespace Kela.Web.Helpers;

public sealed class ApiSessionExpiredException(int statusCode) : Exception
{
    public int StatusCode { get; } = statusCode;
}
