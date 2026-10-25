using Teledok.Domain.Entities;

namespace Teledok.Application.Abstractions;

public interface IClientsRepository
{
    void Add(Client client);
}
