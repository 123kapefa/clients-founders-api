using CSharpFunctionalExtensions;

using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Founders.Responses;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Founders.Queries.GetFounderById;

public sealed class GetFounderByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<FounderDetailsResponse, GetFounderByIdQuery>
{
    public async Task<Result<FounderDetailsResponse, Error>> Handle(GetFounderByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        FounderResponse? founder = await readDbContext.Founders
            .Where(x => x.Id == query.Id)
            .Select(x => new FounderResponse(
                x.Id,
                x.Inn.Value,
                x.FullName.LastName,
                x.FullName.FirstName,
                x.FullName.MiddleName,
                x.CreatedAt,
                x.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (founder is null)
        {
            return FounderErrors.NotFound();
        }

        List<FounderClientResponse> clients = await readDbContext.LegalEntityFounders
            .Where(link => link.FounderId == query.Id)
            .Join(
                readDbContext.Clients,
                link => link.LegalEntityId,
                client => client.Id,
                (link, client) => new { link.CreatedAt, link.Id, Client = client })
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Select(x => new FounderClientResponse(
                x.Client.Id,
                x.Client.Inn.Value,
                x.Client.Name.Value,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new FounderDetailsResponse(
            founder.Id,
            founder.Inn,
            founder.LastName,
            founder.FirstName,
            founder.MiddleName,
            founder.CreatedAt,
            founder.UpdatedAt,
            clients);
    }
}
