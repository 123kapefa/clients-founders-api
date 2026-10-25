using CSharpFunctionalExtensions;

using FluentValidation;
using FluentValidation.Results;

using Teledok.Application.Abstractions;
using Teledok.Application.Common;
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
    FounderResolver founderResolver,
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager,
    IValidator<CreateClientRequest> validator) : ICommandHandler<Guid, CreateClientCommand>
{
    public async Task<Result<Guid, Error>> Handle(CreateClientCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        CreateClientRequest request = command.Request;

        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

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

        if (type == ClientType.INDIVIDUAL_ENTREPRENEUR && request.Founders is { Count: > 0 })
        {
            errors.AddRange(ClientErrors.FoundersNotSupported().Messages);
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

        Result<List<Founder>, Error> founders = await ResolveFoundersAsync(request.Founders ?? [], cancellationToken);
        if (founders.IsFailure)
        {
            return founders.Error;
        }

        Result<Client, Error> client = CreateClient(type!.Value, inn.Value, name.Value, founders.Value);
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

    private async Task<Result<List<Founder>, Error>> ResolveFoundersAsync(
    IReadOnlyList<AddFounderRequest> requests, CancellationToken cancellationToken)
    {
        List<Founder> founders = [];

        for (int i = 0; i < requests.Count; i++)
        {
            Result<Founder, Error> founder = await founderResolver.ResolveAsync(requests[i], cancellationToken);
            if (founder.IsFailure)
            {
                return founder.Error.WithFieldPrefix($"founders[{i}]");
            }

            founders.Add(founder.Value);
        }

        return founders;
    }

    private static ClientType? ParseType(string? raw) =>
        raw switch
        {
            nameof(ClientType.LEGAL_ENTITY) => ClientType.LEGAL_ENTITY,
            nameof(ClientType.INDIVIDUAL_ENTREPRENEUR) => ClientType.INDIVIDUAL_ENTREPRENEUR,
            _ => null,
        };

    private static Result<Client, Error> CreateClient(ClientType type, Inn inn, ClientName name, List<Founder> founders) =>
        type == ClientType.LEGAL_ENTITY
            ? LegalEntity.Create(inn, name, founders).Map(Client (legalEntity) => legalEntity)
            : IndividualEntrepreneur.Create(inn, name).Map(Client (entrepreneur) => entrepreneur);
}
