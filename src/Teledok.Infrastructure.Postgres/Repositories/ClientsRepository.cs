using Teledok.Application.Abstractions;
using Teledok.Domain.Entities;

namespace Teledok.Infrastructure.Postgres.Repositories;

internal sealed class ClientsRepository(TeledokDbContext dbContext) : IClientsRepository
{
    public void Add(Client client) => dbContext.Clients.Add(client);
}
