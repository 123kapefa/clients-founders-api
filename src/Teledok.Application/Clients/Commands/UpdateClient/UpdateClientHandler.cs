using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Clients.Responses;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands.UpdateClient;

public sealed class UpdateClientHandler(
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager) : ICommandHandler<ClientResponse, UpdateClientCommand>
{
    public async Task<Result<ClientResponse, Error>> Handle(UpdateClientCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Result<ClientName, Error> name = ClientName.Of(command.Request.Name);
        if (name.IsFailure)
        {
            return name.Error;
        }

        Client? client = await clientsRepository.GetByIdAsync(command.Id, cancellationToken);
        if (client is null)
        {
            return ClientErrors.NotFound();
        }

        client.Rename(name.Value);

        UnitResult<Error> save = await transactionManager.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save.Error;
        }

        return new ClientResponse(
            client.Id,
            client.Type.ToString(),
            client.Inn.Value,
            client.Name.Value,
            client.CreatedAt,
            client.UpdatedAt);
    }
}
