using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Founders.Commands.DeleteFounder;

public sealed class DeleteFounderHandler(
    IFoundersRepository foundersRepository,
    ITransactionManager transactionManager) : ICommandHandler<DeleteFounderCommand>
{
    public async Task<UnitResult<Error>> Handle(DeleteFounderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Founder? founder = await foundersRepository.GetByIdAsync(command.Id, cancellationToken);
        if (founder is null)
        {
            return FounderErrors.NotFound();
        }

        if (await foundersRepository.HasActiveLinksAsync(founder.Id, cancellationToken))
        {
            return FounderErrors.HasActiveLinks();
        }

        foundersRepository.Remove(founder);

        return await transactionManager.SaveChangesAsync(cancellationToken);
    }
}
