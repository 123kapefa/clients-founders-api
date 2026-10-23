using CSharpFunctionalExtensions;

using Teledok.Shared.SharedKernel;

namespace Teledok.Shared.Core.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
where TCommand : ICommand
{
    Task<Result<TResponse, Error>> Handle(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
where TCommand : ICommand
{
    Task<UnitResult<Error>> Handle(TCommand command, CancellationToken cancellationToken = default);
}
