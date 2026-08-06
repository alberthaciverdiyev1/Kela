using System.Security.Claims;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Kela.Application.Features.Users;

public sealed class KelaUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<User, IdentityRole<int>>
{
    public KelaUserClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var nameClaimType = Options.ClaimsIdentity.UserNameClaimType;
        var existing = identity.FindFirst(nameClaimType);
        if (existing is not null)
        {
            identity.RemoveClaim(existing);
        }

        identity.AddClaim(new Claim(nameClaimType, $"{user.FirstName} {user.LastName}".Trim()));

        return identity;
    }
}
