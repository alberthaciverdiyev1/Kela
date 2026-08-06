using Kela.Application.Users.Dtos;
using Kela.Domain.Entities;

namespace Kela.Application.Users;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.Status,
        user.CreatedAt
        );
}
