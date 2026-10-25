using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;

namespace Teledok.Infrastructure.Postgres.Repositories;

internal sealed class FoundersRepository(TeledokDbContext dbContext) : IFoundersRepository
{
    public void Add(Founder founder) => dbContext.Founders.Add(founder);

    public void Remove(Founder founder) => dbContext.Founders.Remove(founder);

    public Task<Founder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Founders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Founder?> GetByInnAsync(Inn inn, CancellationToken cancellationToken = default) =>
        dbContext.Founders.FirstOrDefaultAsync(x => x.Inn == inn, cancellationToken);

    public Task<bool> HasActiveLinksAsync(Guid founderId, CancellationToken cancellationToken = default) =>
        dbContext.LegalEntityFounders.AnyAsync(x => x.FounderId == founderId, cancellationToken);

}
