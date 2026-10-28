using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;

namespace Edutor.App.Shared.Services;

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
