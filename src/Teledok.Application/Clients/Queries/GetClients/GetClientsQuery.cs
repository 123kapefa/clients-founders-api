using Teledok.Contracts.Clients.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Queries.GetClients;

public sealed record GetClientsQuery(GetClientsRequest Request) : IQuery;
