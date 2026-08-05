using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUserById;

internal sealed class GetUserByIdQueryHandler(IUserRepository users)
    : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(query.Id, cancellationToken);
        return user?.ToDto();
    }
}
