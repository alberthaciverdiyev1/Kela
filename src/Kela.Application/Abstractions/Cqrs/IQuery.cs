using MediatR;

namespace Kela.Application.Abstractions.Cqrs;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
