using Kela.Application.Features.Users.Responses;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user, Role role) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email ?? string.Empty,
        role,
        user.Status,
        user.CreatedAt);

    /// <summary>
    /// Kullanıcının rolünü yalnızca Identity üyeliğinden (AspNetUserRoles) okur.
    /// Rolün başka bir temsili yoktur. Üyelik boşsa güvenli varsayılan Student'tır.
    /// </summary>
    public static async Task<Role> ResolveRoleAsync(this User user, UserManager<User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            if (Enum.TryParse<Role>(roleName, out var role))
            {
                return role;
            }
        }

        return Role.Student;
    }
}
