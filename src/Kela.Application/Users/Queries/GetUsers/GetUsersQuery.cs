using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;
