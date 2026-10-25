using CSharpFunctionalExtensions;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Founders.Responses;
using Teledok.Domain.Entities;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Founders.Commands.UpdateFounder;

public sealed class UpdateFounderHandler(
    IFoundersRepository foundersRepository,
    ITransactionManager transactionManager) : ICommandHandler<FounderResponse, UpdateFounderCommand>
{
    public async Task<Result<FounderResponse, Error>> Handle(UpdateFounderCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Result<FullName, Error> fullName = FullName.Of(
            command.Request.LastName,
            command.Request.FirstName,
            command.Request.MiddleName);
        if (fullName.IsFailure)
        {
            return fullName.Error;
        }

        Founder? founder = await foundersRepository.GetByIdAsync(command.Id, cancellationToken);
        if (founder is null)
        {
            return FounderErrors.NotFound();
        }

        founder.Rename(fullName.Value);

        UnitResult<Error> save = await transactionManager.SaveChangesAsync(cancellationToken);
        if (save.IsFailure)
        {
            return save.Error;
        }

        return new FounderResponse(
            founder.Id,
            founder.Inn.Value,
            founder.FullName.LastName,
            founder.FullName.FirstName,
            founder.FullName.MiddleName,
            founder.CreatedAt,
            founder.UpdatedAt);
    }
}
