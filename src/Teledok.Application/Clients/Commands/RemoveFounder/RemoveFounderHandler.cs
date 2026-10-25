using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands.RemoveFounder;

public sealed class RemoveFounderHandler(
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager) : ICommandHandler<RemoveFounderCommand>
{
    public async Task<UnitResult<Error>> Handle(RemoveFounderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Client? client = await clientsRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
        {
            return ClientErrors.NotFound();
        }

        if (client is not LegalEntity legalEntity)
        {
            return ClientErrors.FoundersNotSupported();
        }

        UnitResult<Error> removed = legalEntity.RemoveFounder(command.FounderId);
        if (removed.IsFailure)
        {
            return removed;
        }

        return await transactionManager.SaveChangesAsync(cancellationToken);
    }
}
