namespace Teledok.Contracts.Clients.Responses;

public sealed record ClientFounderResponse(
    Guid FounderId,
    string Inn,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateTime AddedAt);
