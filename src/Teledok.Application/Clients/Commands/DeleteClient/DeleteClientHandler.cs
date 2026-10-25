using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands.DeleteClient;

public sealed class DeleteClientHandler(
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager) : ICommandHandler<DeleteClientCommand>
{
    public async Task<UnitResult<Error>> Handle(DeleteClientCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Client? client = await clientsRepository.GetByIdAsync(command.Id, cancellationToken);
        if (client is null)
        {
            return ClientErrors.NotFound();
        }

        clientsRepository.Remove(client);

        return await transactionManager.SaveChangesAsync(cancellationToken);
    }
}
