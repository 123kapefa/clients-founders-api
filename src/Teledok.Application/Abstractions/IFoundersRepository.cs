using Teledok.Domain.Entities;
using Teledok.Domain.ValueObjects;

namespace Teledok.Application.Abstractions;

public interface IFoundersRepository
{
    void Add(Founder founder);

    Task<Founder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Founder?> GetByInnAsync(Inn inn, CancellationToken cancellationToken = default);
}
