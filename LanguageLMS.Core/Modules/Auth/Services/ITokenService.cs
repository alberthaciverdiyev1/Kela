using LanguageLMS.Core.Modules.Auth.Entities;

namespace LanguageLMS.Core.Modules.Auth.Services;

public interface ITokenService
{
    string CreateToken(Student student);
}
