using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid Id) : IQuery;
