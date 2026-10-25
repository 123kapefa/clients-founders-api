using CSharpFunctionalExtensions;

using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Contracts.Clients.Responses;
using Teledok.Domain.Errors;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdHandler(IReadDbContext readDbContext)
    : IQueryHandler<ClientResponse, GetClientByIdQuery>
{
    public async Task<Result<ClientResponse, Error>> Handle(GetClientByIdQuery query, CancellationToken cancellationToken = default)
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

        return client is null
            ? ClientErrors.NotFound()
            : client;
    }
}
