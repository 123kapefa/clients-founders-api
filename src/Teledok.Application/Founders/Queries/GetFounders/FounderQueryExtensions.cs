using Teledok.Domain.Entities;

namespace Teledok.Application.Founders.Queries.GetFounders;

internal static class FounderQueryExtensions
{
    public static IQueryable<Founder> WhereMatches(this IQueryable<Founder> founders, string search) =>
        founders.Where(x =>
            ((string)(object)x.Inn).StartsWith(search)
            || x.FullName.LastName.Contains(search)
            || x.FullName.FirstName.Contains(search)
            || (x.FullName.MiddleName != null && x.FullName.MiddleName.Contains(search)));
}
