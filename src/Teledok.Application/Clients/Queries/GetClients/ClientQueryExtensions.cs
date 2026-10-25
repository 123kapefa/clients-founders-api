using Teledok.Domain.Entities;

namespace Teledok.Application.Clients.Queries.GetClients;

internal static class ClientQueryExtensions
{
    public static IQueryable<Client> WhereMatches(this IQueryable<Client> clients, string search) =>
        clients.Where(x =>
            ((string)(object)x.Inn).StartsWith(search)
            || x.Name.Value.Contains(search));
}
