using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Founders.Queries.GetFounderById;

public sealed record GetFounderByIdQuery(Guid Id) : IQuery;
