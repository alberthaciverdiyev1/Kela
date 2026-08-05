using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Pagination;
using Kela.Application.Repositories;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(IUserRepository users)
    : IQueryHandler<GetUsersQuery, PaginatedResult<UserDto>>
{
    public async Task<PaginatedResult<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var result = await users.GetPageAsync(query.Page, query.PageSize, cancellationToken);
        return new PaginatedResult<UserDto>(
            result.Items.Select(u => u.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}
