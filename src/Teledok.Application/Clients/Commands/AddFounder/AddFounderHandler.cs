using CSharpFunctionalExtensions;

using FluentValidation;
using FluentValidation.Results;

using Teledok.Application.Abstractions;
using Teledok.Application.Common;
using Teledok.Contracts.Clients.Requests;
using Teledok.Contracts.Clients.Responses;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands.AddFounder;

public sealed class AddFounderHandler(
    FounderResolver founderResolver,
    IClientsRepository clientsRepository,
    ITransactionManager transactionManager,
    IValidator<AddFounderRequest> validator) : ICommandHandler<ClientFounderResponse, AddFounderCommand>
{
    public async Task<Result<ClientFounderResponse, Error>> Handle(AddFounderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        AddFounderRequest request = command.Request;

        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        Client? client = await clientsRepository.GetByIdAsync(command.ClientId, cancellationToken);
        if (client is null)
        {
            return ClientErrors.NotFound();
        }

        if (client is not LegalEntity legalEntity)
        {
            return ClientErrors.FoundersNotSupported();
        }

        Result<Founder, Error> founder = await founderResolver.ResolveAsync(request, cancellationToken);
        if (founder.IsFailure)
        {
            return founder.Error;
        }

        UnitResult<Error> added = legalEntity.AddFounder(founder.Value);
        if (added.IsFailure)
        {
            return added.Error;
        }

        UnitResult<Error> save = await transactionManager.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save.Error;
        }

        LegalEntityFounder link = legalEntity.Founders.Single(x => x.FounderId == founder.Value.Id);

        return new ClientFounderResponse(
            founder.Value.Id,
            founder.Value.Inn.Value,
            founder.Value.FullName.LastName,
            founder.Value.FullName.FirstName,
            founder.Value.FullName.MiddleName,
            link.CreatedAt);
    }
}
