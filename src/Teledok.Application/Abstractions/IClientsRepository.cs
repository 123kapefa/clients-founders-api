using Teledok.Domain.Entities;

namespace Teledok.Application.Abstractions;

public interface IClientsRepository
{
    void Add(Client client);

    void Remove(Client client);

    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
