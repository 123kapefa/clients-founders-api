using CSharpFunctionalExtensions;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Application.Common;
using Teledok.Contracts.Founders.Requests;
using Teledok.Contracts.Founders.Responses;
using Teledok.Domain.Entities;
using Teledok.Shared.Core.Abstractions;
using Teledok.Shared.SharedKernel;

namespace Teledok.Application.Founders.Queries.GetFounders;

public sealed class GetFoundersHandler(
    IReadDbContext readDbContext,
    IValidator<GetFoundersRequest> validator) : IQueryHandler<PageResult<FounderResponse>, GetFoundersQuery>
{
    public async Task<Result<PageResult<FounderResponse>, Error>> Handle(
        GetFoundersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        GetFoundersRequest request = query.Request;

        ValidationResult validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return validation.ToError();
        }

        int page = request.Page ?? PageResult.DEFAULT_PAGE;
        int pageSize = request.PageSize ?? PageResult.DEFAULT_PAGE_SIZE;

        IQueryable<Founder> founders = readDbContext.Founders;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            founders = founders.WhereMatches(request.Search.Trim());
        }

        int totalCount = await founders.CountAsync(cancellationToken);

        List<FounderResponse> items = await founders
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FounderResponse(
                x.Id,
                x.Inn.Value,
                x.FullName.LastName,
                x.FullName.FirstName,
                x.FullName.MiddleName,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PageResult.Create(items, page, pageSize, totalCount);
    }
}
