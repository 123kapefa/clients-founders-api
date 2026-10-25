using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Clients.Requests;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Commands;

public sealed class FounderResolver(IFoundersRepository foundersRepository)
{
    public async Task<Result<Founder, Error>> ResolveAsync(AddFounderRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.FounderId is { } founderId
            ? await FindExistingAsync(founderId, cancellationToken)
            : await FindOrCreateAsync(request, cancellationToken);
    }

    private async Task<Result<Founder, Error>> FindExistingAsync(Guid founderId, CancellationToken cancellationToken)
    {
        Founder? founder = await foundersRepository.GetByIdAsync(founderId, cancellationToken);

        return founder is null
            ? FounderErrors.NotFound()
            : founder;
    }

    private async Task<Result<Founder, Error>> FindOrCreateAsync(AddFounderRequest request, CancellationToken cancellationToken)
    {
        List<ErrorMessage> errors = [];

        Result<Inn, Error> inn = Inn.Of(request.Inn);
        if (inn.IsFailure)
        {
            errors.AddRange(inn.Error.Messages);
        }

        Result<FullName, Error> fullName = FullName.Of(request.LastName, request.FirstName, request.MiddleName);
        if (fullName.IsFailure)
        {
            errors.AddRange(fullName.Error.Messages);
        }

        if (errors.Count > 0)
        {
            return Error.Validation(errors);
        }

        Founder? existing = await foundersRepository.GetByInnAsync(inn.Value, cancellationToken);
        if (existing is not null)
        {
            return existing.FullName == fullName.Value
                ? existing
                : FounderErrors.FullNameMismatch();
        }

        Result<Founder, Error> created = Founder.Create(inn.Value, fullName.Value);
        if (created.IsSuccess)
        {
            foundersRepository.Add(created.Value);
        }

        return created;
    }
}
