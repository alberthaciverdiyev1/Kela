using Kela.Application.Users.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Users;

public static class UserMappings
{
    public static UserResponse ToResponse(this User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.Status,
        user.CreatedAt
        );
}
