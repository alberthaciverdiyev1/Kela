using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Services;

public sealed class AuthState
{
    public string? Token { get; private set; }
    public StudentDto? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public event Action? Changed;

    public void Set(string token, StudentDto user)
    {
        Token = token;
        User = user;
        Changed?.Invoke();
    }

    public void SetUser(StudentDto user)
    {
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
