using Teledok.Contracts.Founders.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Founders.Queries.GetFounders;

public sealed record GetFoundersQuery(GetFoundersRequest Request) : IQuery;
