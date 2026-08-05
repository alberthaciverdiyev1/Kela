using MediatR;

namespace Kela.Application.Abstractions.Cqrs;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
