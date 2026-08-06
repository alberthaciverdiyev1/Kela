namespace Kela.Application.Users.Auth;

public interface IAuthService
{
    /// <summary>Geçersiz kimlik bilgisi → null döner (endpoint 401'e çevirir).</summary>
    Task<LoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
