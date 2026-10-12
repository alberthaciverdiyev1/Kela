using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services;

public sealed class AuthState
{
    public string? Token { get; private set; }
    public Student? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public event Action? Changed;

    public void Set(string token, Student user)
    {
        Token = token;
        User = user;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        User = null;
        Changed?.Invoke();
    }
}
