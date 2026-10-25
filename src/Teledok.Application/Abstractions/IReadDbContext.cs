using Teledok.Domain.Entities;

namespace Teledok.Application.Abstractions;

public interface IReadDbContext
{
    IQueryable<Client> Clients { get; }

    IQueryable<Founder> Founders { get; }

    IQueryable<LegalEntityFounder> LegalEntityFounders { get; }
}
