using Kela.Application.Features.Users.Responses;
using Kela.Domain.Common;
using Kela.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Users;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user, string role) => new(
        user.Id,
        user.FirstName,
        user.LastName ?? string.Empty,
        user.Email ?? string.Empty,
        role,
        user.Status,
        user.CreatedAt);

    public static async Task<string> ResolveRoleAsync(this User user, UserManager<User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);

        foreach (var roleName in roles)
        {
            if (RoleNames.IsValid(roleName))
            {
                return roleName;
            }
        }

        return RoleNames.Student;
    }
}
