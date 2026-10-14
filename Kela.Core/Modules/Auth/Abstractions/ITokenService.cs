namespace Kela.Core.Modules.Auth.Abstractions;

public interface ITokenService
{
    string CreateToken(long id, string email, string fullName, string role, long tenantId);
}
