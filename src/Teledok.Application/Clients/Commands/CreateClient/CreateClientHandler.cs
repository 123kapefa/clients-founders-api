using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Clients.Requests;
using Teledok.Domain.Entities;
using Teledok.Domain.Enums;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands.CreateClient;

public sealed class CreateClientHandler(
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager) : ICommandHandler<Guid, CreateClientCommand>
{
    public async Task<Result<Guid, Error>> Handle(CreateClientCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        CreateClientRequest request = command.Request;
        List<ErrorMessage> errors = [];

        ClientType? type = ParseType(request.Type);
        if (type is null)
        {
            errors.AddRange(ClientErrors.TypeInvalid().Messages);
        }

        Result<Inn, Error> inn = Inn.Of(request.Inn);
        if (inn.IsFailure)
        {
            errors.AddRange(inn.Error.Messages);
        }

        Result<ClientName, Error> name = ClientName.Of(request.Name);
        if (name.IsFailure)
        {
            errors.AddRange(name.Error.Messages);
        }

        if (errors.Count > 0)
        {
            return Error.Validation(errors);
        }

        Result<Client, Error> client = CreateClient(type!.Value, inn.Value, name.Value);
        if (client.IsFailure)
        {
            return client.Error;
        }

        clientsRepository.Add(client.Value);

        UnitResult<Error> save = await transactionManager.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save.Error;
        }

        return client.Value.Id;
    }

    private static ClientType? ParseType(string? raw) =>
        raw switch
        {
            nameof(ClientType.LEGAL_ENTITY) => ClientType.LEGAL_ENTITY,
            nameof(ClientType.INDIVIDUAL_ENTREPRENEUR) => ClientType.INDIVIDUAL_ENTREPRENEUR,
            _ => null,
        };

    private static Result<Client, Error> CreateClient(ClientType type, Inn inn, ClientName name) =>
        type == ClientType.LEGAL_ENTITY
            ? LegalEntity.Create(inn, name).Map(Client (legalEntity) => legalEntity)
            : IndividualEntrepreneur.Create(inn, name).Map(Client (entrepreneur) => entrepreneur);
}
