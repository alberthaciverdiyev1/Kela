using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Repositories;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(IUserRepository users)
    : IQueryHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var all = await users.GetAllAsync(cancellationToken);
        return all.Select(u => u.ToDto()).ToList();
    }
}
