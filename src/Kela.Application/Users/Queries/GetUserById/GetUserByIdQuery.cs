using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Users.Dtos;

namespace Kela.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(int Id) : IQuery<UserDto?>;
