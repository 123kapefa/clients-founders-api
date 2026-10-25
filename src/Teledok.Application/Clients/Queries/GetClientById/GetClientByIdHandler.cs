using CSharpFunctionalExtensions;

using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Clients.Responses;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<ClientDetailsResponse, GetClientByIdQuery>
{
    public async Task<Result<ClientDetailsResponse, Error>> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        ClientResponse? client = await readDbContext.Clients
            .Where(x => x.Id == query.Id)
            .Select(x => new ClientResponse(
                x.Id,
                x.Type.ToString(),
                x.Inn.Value,
                x.Name.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (client is null)
        {
            return ClientErrors.NotFound();
        }

        List<ClientFounderResponse> founders = await readDbContext.LegalEntityFounders
            .Where(x => x.LegalEntityId == query.Id)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new ClientFounderResponse(
                x.FounderId,
                x.Founder.Inn.Value,
                x.Founder.FullName.LastName,
                x.Founder.FullName.FirstName,
                x.Founder.FullName.MiddleName,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ClientDetailsResponse(
            client.Id,
            client.Type,
            client.Inn,
            client.Name,
            client.CreatedAt,
            client.UpdatedAt,
            founders);
    }
}
