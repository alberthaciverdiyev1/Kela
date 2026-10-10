namespace LanguageLMS.Core.Modules.Auth.Services;

public interface ITokenService
{
    string CreateToken(long id, string email, string fullName, string actor);
}
