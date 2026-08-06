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
    /// Kullanıcının rolünü Identity üyeliğinden (AspNetUserRoles) çözer.
    /// Üyelik yoksa profilden türetir (tutarlılık garantisi: her profilin bir rolü var).
    /// </summary>
    public static async Task<Role> ResolveRoleAsync(this User user, UserManager<User> userManager)
    {
        var roles = await userManager.GetRolesAsync(user);
        if (roles.FirstOrDefault() is { } roleName && Enum.TryParse<Role>(roleName, out var role))
        {
            return role;
        }

        if (user.Teacher is not null) return Role.Teacher;
        if (user.Student is not null) return Role.Student;
        if (user.Parent is not null) return Role.Parent;
        return Role.Admin;
    }
}
