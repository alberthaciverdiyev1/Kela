using Kela.Application.Features.Users.Auth.Requests;
using Kela.Application.Features.Users.Auth.Responses;
using Kela.Application.Validation;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users.Auth;

internal sealed class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IValidator<LoginRequest> loginValidator) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        loginValidator.Validate(request);

        var user = await userManager.FindByEmailAsync(request.Email.Trim());

        // Kullanıcı yoksa da aynı sonucu dön → user enumeration koruması.
        if (user is null || user.Status != UserStatus.Active)
        {
            return null;
        }

        // PasswordSignInAsync tek çağrıda:
        //   - parolayı Identity hasher'ıyla (PBKDF2) doğrular,
        //   - lockout sayacını yönetir (5 hatalı deneme → 5 dk kilit),
        //   - başarıda security stamp içeren Identity cookie'sini yazar;
        //     böylece parola değişince eski oturum geçersiz olur.
        var result = await signInManager.PasswordSignInAsync(
            user, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return null;
        }

        var role = await user.ResolveRoleAsync(userManager);

        return new LoginResponse(user.Id, user.FirstName, user.LastName, role);
    }
}
