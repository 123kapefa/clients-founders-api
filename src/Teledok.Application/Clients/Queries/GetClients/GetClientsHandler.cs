using CSharpFunctionalExtensions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Application.Common;
using Teledok.Contracts.Clients.Requests;
using Teledok.Contracts.Clients.Responses;
using Teledok.Domain.Entities;
using Teledok.Domain.Enums;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Clients.Queries.GetClients;

public sealed class GetClientsHandler(
    IReadDbContext readDbContext,
    IValidator<GetClientsRequest> validator) : IQueryHandler<PageResult<ClientResponse>, GetClientsQuery>
{
    public async Task<Result<PageResult<ClientResponse>, Error>> Handle(
        GetClientsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        GetClientsRequest request = query.Request;

        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        int page = request.Page ?? PageResult.DEFAULT_PAGE;
        int pageSize = request.PageSize ?? PageResult.DEFAULT_PAGE_SIZE;

        IQueryable<Client> clients = ApplyFilters(readDbContext.Clients, request);

        int totalCount = await clients.CountAsync(cancellationToken);

        List<ClientResponse> items = await clients
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClientResponse(
                x.Id,
                x.Type.ToString(),
                x.Inn.Value,
                x.Name.Value,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PageResult.Create(items, page, pageSize, totalCount);
    }

    private static IQueryable<Client> ApplyFilters(IQueryable<Client> clients, GetClientsRequest request)
    {
        if (request.Type is not null)
        {
            ClientType type = Enum.Parse<ClientType>(request.Type);
            clients = clients.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            clients = clients.WhereMatches(request.Search.Trim());
        }

        return clients;
    }
}
