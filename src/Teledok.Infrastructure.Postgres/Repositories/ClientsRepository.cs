using Microsoft.EntityFrameworkCore;

using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;

namespace Teledok.Infrastructure.Postgres.Repositories;

internal sealed class ClientsRepository(TeledokDbContext dbContext) : IClientsRepository
{
    public void Add(Client client) => dbContext.Clients.Add(client);

    public void Remove(Client client) => dbContext.Clients.Remove(client);

    public Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Clients
            .Include(x => ((LegalEntity)x).Founders)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
