namespace Teledok.Contracts.Clients.Responses;

public sealed record ClientDetailsResponse(
    Guid Id,
    string Type,
    string Inn,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ClientFounderResponse> Founders);
