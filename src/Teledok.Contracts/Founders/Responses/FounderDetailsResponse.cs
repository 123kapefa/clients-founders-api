namespace Teledok.Contracts.Founders.Responses;

public sealed record FounderDetailsResponse(
    Guid Id,
    string Inn,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<FounderClientResponse> Clients);
