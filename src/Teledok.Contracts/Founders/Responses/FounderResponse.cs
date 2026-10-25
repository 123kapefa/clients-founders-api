namespace Teledok.Contracts.Founders.Responses;

public sealed record FounderResponse(
    Guid Id,
    string Inn,
    string LastName,
    string FirstName,
    string? MiddleName,
    DateTime CreatedAt,
    DateTime UpdatedAt);
