namespace Teledok.Contracts.Clients.Responses;

public sealed record ClientResponse(
    Guid Id,
    string Type,
    string Inn,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt);
