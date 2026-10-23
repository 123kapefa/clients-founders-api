using CSharpFunctionalExtensions;

using Teledok.Shared.SharedKernel;

namespace Teledok.Shared.Core.Database;

public interface ITransactionManager
{
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default);
}
