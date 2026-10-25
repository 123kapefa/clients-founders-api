using CSharpFunctionalExtensions;

using Teledok.Shared.SharedKernel;

namespace Teledok.Shared.Core.Abstractions;

public interface IQueryHandler<TResponse, in TQuery> where TQuery : IQuery
{
    Task<Result<TResponse, Error>> Handle(TQuery query, CancellationToken cancellationToken = default);
}
