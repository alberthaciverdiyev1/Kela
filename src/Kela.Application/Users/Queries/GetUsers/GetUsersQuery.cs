using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Pagination;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 20) : IQuery<PaginatedResult<UserDto>>;
